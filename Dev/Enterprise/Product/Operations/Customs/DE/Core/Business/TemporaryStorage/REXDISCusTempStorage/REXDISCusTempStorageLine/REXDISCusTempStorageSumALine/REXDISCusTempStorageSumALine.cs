using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(REXDISCusTempStorageDec), "CusTempStorageLines")]
	public class REXDISCusTempStorageSumALine : REXDISCusTempStorageLine
	{
		public REXDISCusTempStorageSumALine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "This would resolve to an abstract class which is undesirable")]
		public new class Schema : REXDISCusTempStorageLine.Schema
		{
			public const string ReferenceNumber = "ReferenceNumber";
		}

		#region Construction / Loading

		public static REXDISCusTempStorageSumALine LoadOrCreate(REXDISCusTempStorageReExportLine reExportLine)
		{
			return Load(reExportLine) ?? New(reExportLine);
		}

		static REXDISCusTempStorageSumALine Load(REXDISCusTempStorageReExportLine reExportLine)
		{
			var query = new ZDBOnlyQuery(typeof(CusTempStorageLine));
			query.OrderBy = CusTempStorageLineSchema.TSL_SystemCreateTimeUtc.Name;
			query.FetchOnlyFromLocalCache = !reExportLine.IsInDatabase;
			var subquery = new ZDBOnlySubQuery(typeof(CusTempStorageLinePivot), CusTempStorageLinePivotSchema.SLR_TSL_ToLine);
			subquery.AddToFilter(CusTempStorageLinePivotSchema.SLR_TSL_FromLine, reExportLine.PK);
			query.AddSubQuery(subquery, JoinCondition.And);
			return reExportLine.Factory.LoadTop1<REXDISCusTempStorageSumALine>(query);
		}

		static REXDISCusTempStorageSumALine New(REXDISCusTempStorageReExportLine reExportLine) => reExportLine.CusTempStorageSumALines.AddNew();

		#endregion

		#region Properties

		[BusinessObjectTestExclude]
		[ResourceStringData("6D5C8FEE-7EDC-4C92-82C5-BDE858CE7C51", Caption = "Reference")]
		public ZString ReferenceNumber
		{
			get
			{
				var result = TSL_ReferenceNumber;
				if (!result.IsEmpty && IsREGDeclaration)
				{
					result = SumARegistrationNumberFormatter.Format(result);
				}
				return result;
			}
			set
			{
				value = value.KeepAlphanumericCharacters();
				var hasChanged = TSL_ReferenceNumber != value;
				if (hasChanged)
				{
					TSL_ReferenceNumber = value;
					Validation.ValidateReferenceNumber();
					ReferenceNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ReferenceNumberInfo => GetZPropertyInfo(Schema.ReferenceNumber);

		[ResourceStringData("EF53BEB0-6B1F-4910-B593-B7B762FACBB4", Caption = "Reference Line No.")]
		public override ZInt TSL_ReferenceNumberLine
		{
			get => base.TSL_ReferenceNumberLine;
			set => base.TSL_ReferenceNumberLine = value;
		}

		#endregion

		#region Validation
		protected override EU.Business.CusTempStorage.CusTempStorageLineValidation GetNewValidation() => new REXDISCusTempStorageSumALineValidation(this);
		public new REXDISCusTempStorageSumALineValidation Validation => (REXDISCusTempStorageSumALineValidation)base.Validation;

		#endregion

		#region Lookups

		protected override EU.Business.CusTempStorage.CusTempStorageLineLookups GetNewLookups() => new REXDISCusTempStorageSumALineLookups(this);

		public new REXDISCusTempStorageSumALineLookups Lookups => (REXDISCusTempStorageSumALineLookups)base.Lookups;

		#endregion
	}
}
