using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.SG
{
	public class CopyTradersRemarksToCusSupportingInfo : DataTransformation
	{
		public override string UserDescription => "Copy SG traders remarks to CusSupportingInfo.";
		public const string TradersRemarksLastClusterKeyWaterMark = "CopyTradersRemarksToCusSupportingInfo_LastClusterKey";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'SG'"))
			{
				if (!int.TryParse(ExtProperty.Database.Select(Db.Connection, TradersRemarksLastClusterKeyWaterMark), out var maxClusterKeyInBatch))
				{
					maxClusterKeyInBatch = GetMaxClusterKey();
				}

				while (maxClusterKeyInBatch > 0)
				{
					var minClusterKeyInBatch = maxClusterKeyInBatch - 50000;
					var updatedLinesNum = ProcessBatch(maxClusterKeyInBatch, minClusterKeyInBatch);
					manager?.ShowInfoMessage($"Batch processed, cluster key range [{maxClusterKeyInBatch}]-[{minClusterKeyInBatch}], updated number of declarations [{updatedLinesNum}].");

					maxClusterKeyInBatch = minClusterKeyInBatch - 1;
					if (maxClusterKeyInBatch > 0)
					{
						ExtProperty.Database.Update(Db.Connection, TradersRemarksLastClusterKeyWaterMark, maxClusterKeyInBatch.ToString());
					}
					token.ThrowIfCancellationRequested();
				}
				ExtProperty.Database.Delete(Db.Connection, TradersRemarksLastClusterKeyWaterMark);
			}
		}

		int ProcessBatch(int maxClusterKeyInBatch, int minClusterKeyInBatch)
		{
			var sql = $@"
SELECT JE_ClusterKey, JE_PK, ST_NoteText
FROM dbo.JobDeclaration WITH (INDEX (NR_UC__JE_ClusterKey))
JOIN dbo.GlbCompany ON JE_GC = GC_PK
JOIN dbo.StmNote WITH (FORCESEEK, INDEX (NR_RC__ST_ParentID))
	ON ST_ParentID = JE_PK
WHERE GC_RN_NKCountryCode = 'SG'
	AND ST_Description = 'SG Traders Remarks'
	AND JE_ClusterKey >= @minClusterKeyInBatch 
	AND JE_ClusterKey <= @maxClusterKeyInBatch
	AND JE_PK NOT IN (SELECT CSI_ParentID FROM dbo.CusSupportingInfo WHERE CSI_Type = 'TRK' AND CSI_RN_NKCountryCode = 'SG' AND CSI_ParentTableCode = 'JE')
OPTION (MAXDOP 1)
			";
			var tradersRemarkList = new List<TradersRemarkBusinessObject>();
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@maxClusterKeyInBatch", SqlDbType.Int, maxClusterKeyInBatch);
				cmd.AddParameter("@minClusterKeyInBatch", SqlDbType.Int, minClusterKeyInBatch);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var tradersRemarkBusinessObject = new TradersRemarkBusinessObject
						{
							JE_ClusterKey = (int)reader["JE_ClusterKey"],
							JE_PK = (Guid)reader["JE_PK"],
							ST_NoteText = (string)reader["ST_NoteText"]
						};

						tradersRemarkList.Add(tradersRemarkBusinessObject);
					}
				}
			}
			foreach (var tradersRemark in tradersRemarkList)
			{
				var remarksTextSplit = new TextSplitElegantly(512, 5);
				remarksTextSplit.Text = tradersRemark.ST_NoteText;
				for (var i = 0; i < 5; i++)
				{
					if (!remarksTextSplit[i].IsNullOrEmpty())
					{
						CreateCusSupportingInfo(tradersRemark.JE_PK, remarksTextSplit[i], i + 1);
					}
				}
			}
			var updatedLinesNum = tradersRemarkList.Count;
			return updatedLinesNum;
		}

		Guid CreateCusSupportingInfo(Guid parentPK, string remark, int lineNo)
		{
			var createCusSupportingInfoSql = $@"
INSERT INTO dbo.CusSupportingInfo (CSI_PK, CSI_Type, CSI_Description, CSI_LineNo, CSI_ParentTableCode, CSI_DataModel, CSI_ParentID, CSI_RN_NKCountryCode, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser)
VALUES (@pk, 'TRK', @description, @lineNo, 'JE', 'SG', @parentId, 'SG', GETUTCDATE(), 'E', GETUTCDATE(), 'E');
			";
			var pk = Guid.NewGuid();
			using (var command = Db.Connection.Command(createCusSupportingInfoSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@description", SqlDbType.VarChar, remark);
				command.AddParameter("@parentId", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@lineNo", SqlDbType.Int, lineNo);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		static int GetMaxClusterKey()
		{
			return Db.Connection.ExecuteScalar<int>(@"
SELECT ISNULL(MAX(JE_ClusterKey), 0) maxClusterKey
FROM dbo.JobDeclaration");
		}

		class TradersRemarkBusinessObject
		{
			public int JE_ClusterKey { get; set; }
			public Guid JE_PK { get; set; }
			public string ST_NoteText { get; set; }
		}
	}

	//The traders remarkes was splitted into message by Enterprise.Edifact.Utilities.TextSplitElegantly.
	//We need to move them into CusSupportInfo exactly as the message sent before.  
	public class TextSplitElegantly : TextSplitter
	{
		public TextSplitElegantly(int maxLength, int maxElements) : base(maxLength)
		{
			base.maxLength = maxLength;
			MaxElements = maxElements;
		}
		public int MaxElements { get; set; }

		#region Implementation

		bool CanSplitElegantly(int elements)
		{
			return Count < elements + 1;
		}

		protected override void Recalculate()
		{
			var chopText = Text.Replace("\r", string.Empty);
			chopText = chopText.TrimEnd();
			while (chopText.Contains("\n\n"))
			{
				chopText = chopText.Replace("\n\n", "\n");
			}
			splitText.Clear();

			while (chopText.Length > 0)
			{
				var startingCharacters = string.Empty;
				var positionToKeep = 0;

				var carriageReturnInStartingCharacters = SubstringSafe(chopText, 0, MaxLength + 1).IndexOf('\n');

				if (carriageReturnInStartingCharacters > -1)
				{
					startingCharacters = SubstringSafe(chopText, 0, carriageReturnInStartingCharacters);
					positionToKeep = carriageReturnInStartingCharacters + 1;
				}
				else
				{
					if (SubstringSafe(chopText, 0, MaxLength).IndexOf(' ') == -1 || chopText.Length < MaxLength + 1)
					{
						startingCharacters = SubstringSafe(chopText, 0, MaxLength);
						positionToKeep = MaxLength;
					}
					else
					{
						for (var i = MaxLength; startingCharacters.IsNullOrEmpty(); i--)
						{
							if (chopText[i] == ' ')
							{
								startingCharacters = SubstringSafe(chopText, 0, i);
								positionToKeep = i + 1;
								break;
							}
						}
					}
				}

				splitText.Add(startingCharacters);
				chopText = SubstringSafe(chopText, positionToKeep, int.MaxValue);
			}

			needsRecalc = false;

			if (!CanSplitElegantly(MaxElements))
			{
				Text = Text.Replace("\r\n", " ").Replace("\t", " ").Replace("\n", " ");   //When cannot split elegantly, remove any imbedded formatting before using standard splitting
				base.Recalculate();
			}
		}

		#endregion
	}

	public class TextSplitter
	{
		public TextSplitter()
		{
			splitText = new ArrayList();
		}

		public TextSplitter(int maxLength)
			: this()
		{
			this.maxLength = maxLength;
		}

		public int MaxLength
		{
			get { return maxLength; }
			set
			{
				maxLength = value;
				needsRecalc = true;
			}
		}

		public string Text
		{
			get { return text; }
			set
			{
				text = TextFormat(value);
				needsRecalc = true;
			}
		}

		protected virtual string TextFormat(string text)
		{
			return text;
		}

		public string this[int index]
		{
			get
			{
				if (needsRecalc)
				{
					Recalculate();
				}

				return index < splitText.Count ? splitText[index].ToString() : string.Empty;
			}
		}

		public int Count
		{
			get
			{
				if (needsRecalc)
				{
					Recalculate();
				}

				return splitText.Count;
			}
		}

		protected virtual void Recalculate()
		{
			var chopText = Text.Replace("\r", string.Empty);
			splitText.Clear();
			while (chopText.Length > 0)
			{
				var startingCharacters = SubstringSafe(chopText, 0, MaxLength);
				var carriageReturnInStartingCharacters = startingCharacters.IndexOf('\n');
				int positionToKeep;
				if (carriageReturnInStartingCharacters > -1)
				{
					startingCharacters = SubstringSafe(chopText, 0, carriageReturnInStartingCharacters);
					positionToKeep = carriageReturnInStartingCharacters + 1;
				}
				else
				{
					positionToKeep = MaxLength;
				}

				splitText.Add(startingCharacters);
				chopText = SubstringSafe(chopText, positionToKeep, int.MaxValue);
			}

			needsRecalc = false;
		}

		public string SubstringSafe(string origString, int startIndex, int length)
		{
			string result = string.Empty;
			if (startIndex < 0)
			{
				startIndex = 0;
			}

			int num = checked(origString.Length - startIndex);
			if (num > 0)
			{
				if (num > length && length >= 0)
				{
					num = length;
				}

				result = origString.Substring(startIndex, num);
			}

			return result;
		}

		protected bool needsRecalc = true;
		protected string text = string.Empty;
		protected int maxLength;
		protected ArrayList splitText;
	}
}
