using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class ExtendedHoursRequestLineValidation : AutoExtendedHoursRequestLineValidation
	{
		public ExtendedHoursRequestLineValidation(AutoExtendedHoursRequestLine parent) : base(parent)
		{
		}

		protected new ExtendedHoursRequestLine Parent => (ExtendedHoursRequestLine)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateFormattedReferenceNumber();
		}
		protected override void CheckReferenceNumberType()
		{
			base.CheckReferenceNumberType();
			var header = Parent.Parent;
			if (header.IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ReferenceNumberTypeInfo);
			}
		}

		public void ValidateFormattedReferenceNumber()
		{
			ValidateCalculatedProperty(Parent.FormattedReferenceNumberInfo);
		}

		protected void CheckFormattedReferenceNumber()
		{
			if (Parent.ReferenceNumber.IsEmpty)
			{
				Parent.FormattedReferenceNumberInfo.AddMessageError(Res.GetString("30E92218-DE37-47DE-8C07-EC89AEB53822", "Please enter an entry number. If you don't have a specific entry number, then please enter 'NO'."));
			}
			var header = Parent.Parent;
			var query = new ZDBOnlyQuery(typeof(CusMiscRequestHeader));
			query.AddToFilter(CusMiscRequestHeaderSchema.CMR_MessageType, header.MessageType);

			var subQuery = new ZDBOnlySubQuery(typeof(CusMiscRequestLine), CusMiscRequestLineSchema.CML_CMR);
			subQuery.AddToFilter(CusMiscRequestLineSchema.CML_EntryType, Parent.ReferenceNumberType);
			subQuery.AddToFilter(CusMiscRequestLineSchema.CML_EntryNumber, Parent.ReferenceNumber);
			query.AddSubQuery(subQuery, JoinCondition.And);

			var headerWithTheSameLine = header.Factory.LoadTop1<CusMiscRequestHeader>(query);
			if (headerWithTheSameLine != null)
			{
				Parent.FormattedReferenceNumberInfo.AddWarning(Res.GetString("4433938E-175D-45B6-A9BC-14AE3E008DCA", "There is another {0} request, {1} which has this entry number. Please check it.", headerWithTheSameLine.CMR_MessageType, headerWithTheSameLine.CMR_JobNumber));
			}
		}

		protected override void CheckHSDescription()
		{
			base.CheckHSDescription();
			var header = Parent.Parent;
			if (header.IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.HSDescriptionInfo);
			}
		}

		protected override void CheckPayerCompanyName()
		{
			base.CheckPayerCompanyName();
			var header = Parent.Parent;
			if (header.IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.PayerCompanyNameInfo);
			}
		}

		protected override void CheckBondedAreaCode()
		{
			base.CheckBondedAreaCode();
			var header = Parent.Parent;
			if (header.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BondedAreaCodeInfo);
			}
		}

		protected override void CheckPackageCount()
		{
			base.CheckPackageCount();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.PackageCountInfo);
		}

		protected override void CheckCustomsValue()
		{
			base.CheckCustomsValue();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.CustomsValueInfo);
		}

		protected override void CheckTotalWeight()
		{
			base.CheckTotalWeight();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.TotalWeightInfo);
		}

		protected override void CheckUQ()
		{
			base.CheckUQ();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.UQInfo);
		}
	}
}
