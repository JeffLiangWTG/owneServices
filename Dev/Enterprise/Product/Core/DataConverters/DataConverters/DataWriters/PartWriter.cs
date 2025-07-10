using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataConverters.CustomsFiles
{
	public abstract class PartWriter : DataWriter
	{
		public PartWriter(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Public Fields to be filled in when importing.
		public ZString DeliveranceSystemIDCode;
		public ZString PartNumber;
		public ZString Description;
		public ZString LookupCode;
		public ZString SupplierCode;
		public ZString ImporterCode;
		public ZString DefaultStockUnit;
		public ZDecimal Weight;
		public ZString WeightUQ;
		public ZDecimal Volume;
		public ZString VolumeUQ;
		#endregion

		#region OrgMatching

		#region ImporterMatch

		protected OrgMatcher ImporterMatch
		{
			get
			{
				if (fImporterMatch == null)
				{
					fImporterMatch = new OrgMatcher(ImporterCode, DeliveranceSystemIDCode, Factory);
				}
				return fImporterMatch;
			}
		}
		OrgMatcher fImporterMatch;

		#endregion

		#region SupplierMatch
		protected OrgMatcher SupplierMatch
		{
			get
			{
				if (fSupplierMatch == null)
				{
					fSupplierMatch = new OrgMatcher(SupplierCode, DeliveranceSystemIDCode, Factory);
				}
				return fSupplierMatch;
			}
		}
		OrgMatcher fSupplierMatch;
		#endregion

		#endregion

		#region RecordDescription
		public override ZString RecordDescription
		{
			get { return "Part: " + PartNumber + " - " + Description; }
		}
		#endregion

		#region PartMatcher
		PartMatcher Matcher
		{
			get
			{
				if (fMatcher == null)
				{
					fMatcher = new PartMatcher(PartNumber, SupplierMatch, ImporterMatch, Factory, GetPartType());
				}
				return fMatcher;
			}
		}
		PartMatcher fMatcher;
		#endregion

		#region GetAnyReasonRecordShouldBeExcluded
		protected override internal ZString GetAnyReasonRecordShouldBeExcluded()
		{
			return PartNumber.IsEmpty ? new ZString("No Part Number On This Row") : Matcher.ReasonPartCannotBeImported;
		}
		#endregion

		#region GetNewBusinessObject
		protected override BusinessObject GetNewBusinessObject()
		{
			var result = (Customs.Business.OrgSupplierPart)Factory.New(GetPartType());
			if (!ImporterMatch.MatchedPK.IsEmpty)
			{
				result.RelatedOrganisations.AddOrganisationIfNotExist(ImporterMatch.MatchedPK, OrgPartRelation.RelationshipTypes.Owner);
			}
			if (!SupplierMatch.MatchedPK.IsEmpty)
			{
				result.RelatedOrganisations.AddOrganisationIfNotExist(SupplierMatch.MatchedPK, OrgPartRelation.RelationshipTypes.Supplier);
			}
			return result;
		}

		#endregion

		#region GetExistingBusinessObject
		protected override BusinessObject GetExistingBusinessObject()
		{
			return Matcher.MatchedPart;
		}
		#endregion

		#region UpdateEnterpriseValues

		protected override void UpdateEnterpriseValues(BusinessObject businessObjectToUpdate)
		{
			Customs.Business.OrgSupplierPart part = (Customs.Business.OrgSupplierPart)businessObjectToUpdate;

			part.OP_PartNum = PartNumber;
			part.OP_Desc = Description;
			part.OP_StockKeepingUnit = DefaultStockUnit;
			part.OP_Weight = Weight;
			part.OP_WeightUQ = WeightUQ;
			part.OP_Cubic = Volume;
			part.OP_CubicUQ = VolumeUQ;

			UpdateCountrySpecificData(part);
		}

		#endregion

		protected abstract Type GetPartType();
		protected abstract void UpdateCountrySpecificData(BusinessObject bizO);
	}
}
