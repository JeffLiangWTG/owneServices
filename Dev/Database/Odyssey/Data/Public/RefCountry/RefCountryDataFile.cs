using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(RefCountrySchema))]
[assembly: UsesConstants(typeof(RefCurrencySchema))]

namespace Enterprise.DbUpgrader.Data
{
	public class RefCountryDataFile : EmbeddedDataFile, IFixReferencesAndDuplicates
	{
		public RefCountryDataFile() : base(DataFileRelativePath, DataFileTables)
		{
		}

		internal RefCountryDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, DataFileTables)
		{
		}

		const string DataFileRelativePath = @"Public\RefCountry\RefCountry.xml.gz";
		public override string ResourceRelativeName => "RefCountry.RefCountry.xml.gz";
		protected static readonly string[] DataFileTables = new string[2]
		{
			RefCountrySchema.Constants.TableName,
			RefCurrencySchema.Constants.TableName
		};

		protected override string SelectQuery
		{
			get
			{
				return @"
					SELECT * FROM dbo.RefCountry ORDER BY 1;
					SELECT * FROM dbo.RefCurrency ORDER BY 1";
			}
		}

		protected override List<UniqueIndexInfo> GetUniqueIndexesToDropBeforeSaveAndRecreateAfterwards()
		{
			return new List<UniqueIndexInfo>
			{
				new ViewUniqueIndexInfo("vw_ZoneCountry", RefZoneHeaderSchema.Constants.FZ_Code + ", " + RefZoneHeaderSchema.Constants.FZ_ZoneType +  ", " + RefCountrySchema.Constants.RN_Code),
			};
		}

		#region PerformExtraDataManipulationBeforeEnableFks

		#region RefCountry PK conflicts

		public void AddCountryPkConflict(Guid oldPk, Guid newPk)
		{
			countryPkConflicts.Add(oldPk, newPk);
		}

		public void PerformExtraDataManipulationBeforeEnablingConstraints()
		{
			return;
		}

		protected Dictionary<Guid, Guid> countryPkConflicts = new Dictionary<Guid, Guid>();

		#endregion

		#endregion
	}
}
