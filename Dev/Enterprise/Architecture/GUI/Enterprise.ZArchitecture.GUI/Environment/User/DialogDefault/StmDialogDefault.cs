using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Core.DialogDefault
{
	public class StmDialogDefault : AutoStmDialogDefault, IComparable<StmDialogDefault>
	{
		public StmDialogDefault(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		/// <summary>
		/// When a users default mimics a companies default (for example, to change the 'Show Dialog' panel,
		/// this will be set to the company's default. Otherwise it will be null
		/// </summary>
		public StmDialogDefault ParentOverride { get; set; }
		public bool TryToFormatXml { get; set; }

		StmDialogDefaultLookups lookups;
		public StmDialogDefaultLookups Lookups
		{
			get { return lookups ?? (lookups = new StmDialogDefaultLookups(this)); }
		}

		#region Properties

		[ReadOnly(true)]
		public ZBool AppliesToSimilarDialogs { get { return SDD_Context.IsEmpty; } }
		public ZPropertyInfo AppliesToSimilarDialogsInfo { get { return GetZPropertyInfo(nameof(AppliesToSimilarDialogs)); } }

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region SDD_ShowDialog

		public override ZBool SDD_ShowDialog
		{
			get { return base.SDD_ShowDialog || SDD_Level != DialogDefaultLevel.Codes.User; }
			set { base.SDD_ShowDialog = value; }
		}

		public bool SDD_ShowDialog_ReadOnly { get { return SDD_Level != DialogDefaultLevel.Codes.User; } }

		public bool SDD_ShowDialog_Base => base.SDD_ShowDialog;

		#endregion

		#region SDD_Owner

		[List("Lookups.Owners")]
		public override ZGuid SDD_Owner
		{
			get { return base.SDD_Owner; }
			set { base.SDD_Owner = value; }
		}

		public bool SDD_Owner_ReadOnly
		{
			get { return SDD_Level == DialogDefaultLevel.Codes.Global || !CanCreateAndModifyGlobalDialogDefaults.IsAllowed; }
		}

		ISecurityCheckpoint CanCreateAndModifyGlobalDialogDefaults
		{
			get { return EnvProxy.Instance.Security.FindCheckPoint("CanCreateAndModifyGlobalDialogDefaults"); }
		}

		#endregion

		#region SDD_Caption

		public override ZString SDD_Caption
		{
			get
			{
				return base.SDD_Caption;
			}
			set
			{
				base.SDD_Caption = value.Length <= Schema.SDD_CaptionMaxLength ? value : value.Substring(0, Schema.SDD_CaptionMaxLength);
			}
		}

		public bool SDD_Caption_ReadOnly { get { return true; } }

		#endregion

		#region SDD_level

		[List("Lookups.Level")]
		public override ZString SDD_Level
		{
			get { return base.SDD_Level; }
			set
			{
				if (value != base.SDD_Level)
				{
					PreviousOwnerValues[base.SDD_Level] = SDD_Owner;
					base.SDD_Level = value;

					//Remember the owner/company pair, so if you change CMP -> GLB then back to CMP it restores the original CMP value
					ZGuid owner;
					SDD_Owner = PreviousOwnerValues.TryGetValue(value, out owner) ? owner : ZGuid.Empty;

					SDD_OwnerInfo.RefreshBinding();
				}
			}
		}

		Dictionary<string, ZGuid> _previousOwnerValues;
		Dictionary<string, ZGuid> PreviousOwnerValues { get { return _previousOwnerValues ?? (_previousOwnerValues = new Dictionary<string, ZGuid>()); } }

		#endregion

		#region SDD_SerializedDefaults

		public override ZString SDD_SerializedDefaults
		{
			get
			{
				if (TryToFormatXml)
				{
					try { return XElement.Parse(base.SDD_SerializedDefaults).ToString(SaveOptions.None); }
					catch (XmlException) { /*Just return unformatted if we can't parse it */ }
				}

				return base.SDD_SerializedDefaults;
			}
			set
			{
				string xml = value;
				if (TryToFormatXml)
				{
					try { xml = XElement.Parse(xml).ToString(SaveOptions.DisableFormatting); }
					catch (XmlException) { /*Probably half way through editing it */ }
				}

				base.SDD_SerializedDefaults = xml;
			}
		}

		public bool SDD_SerializedDefaults_ReadOnly
		{
			get { return !EnvProxy.Instance.Security.FindCheckPoint("CanModifyDefaultsXML").IsAllowed; }
		}

		#endregion

		#endregion

		#region Implementation

		public int CompareTo(StmDialogDefault other)
		{
			var levelCompared = DialogDefaultLevel.LevelCodesInOrderOfInclusivity.IndexOf(SDD_Level) - DialogDefaultLevel.LevelCodesInOrderOfInclusivity.IndexOf(other.SDD_Level);

			var otherContextIsNull = other.SDD_Context == null;
			var thisContextIsNull = SDD_Context == null;

			return levelCompared * 10 + (thisContextIsNull == otherContextIsNull ? 0 : otherContextIsNull ? -1 : 1);
		}

		public override bool Equals(object obj)
		{
			var other = obj as StmDialogDefault;

			if (other == null)
			{
				return false;
			}
			else if (IsDeleted || other.IsDeleted)
			{
				return IsDeleted == other.IsDeleted;
			}
			else
			{
				return SDD_DialogIdentifier == other.SDD_DialogIdentifier &&
					SDD_Level == other.SDD_Level &&
					SDD_Owner == other.SDD_Owner &&
					SDD_Context == other.SDD_Context;
			}
		}

		public override int GetHashCode()
		{
			return (IsDeleted) ?
				GetType().GetHashCode() :
				SDD_DialogIdentifier.GetHashCode() ^ SDD_Level.GetHashCode() ^ SDD_Owner.GetHashCode() ^ SDD_Context.GetHashCode();
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return GetHumanReadableOwnerName() + ": " + SDD_Caption;
			}
		}

		string GetHumanReadableOwnerName()
		{
			var unknown = new Lazy<string>(() => Res.GetString("5BDDE4F5-7350-4FF9-B062-708040740250", "<Unknown owner>"), false);
			switch (SDD_Level)
				{
					case DialogDefaultLevel.Codes.User:
					case DialogDefaultLevel.Codes.Company:
						var owner = Lookups.Owners.FindByPK(SDD_Owner);
						return owner == null ?
							unknown.Value :
							owner.HumanReadableName.ToString();

					case DialogDefaultLevel.Codes.Global:
						return Res.GetString("60B1B6F5-CD5D-4012-91FC-B82BAB22E0D1", "Global");
					default:
						ErrorReporter.ReportOnce("Unknown SDD_Level value '" + SDD_Level + "'");
						return unknown.Value;
				}
		}

		#endregion
	}
}
