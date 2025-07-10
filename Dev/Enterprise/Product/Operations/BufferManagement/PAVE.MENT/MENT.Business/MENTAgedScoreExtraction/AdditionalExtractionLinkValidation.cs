using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.PAVE.MENT.Business
{
	public class AdditionalExtractionLinkValidation : ZValidation
	{
		public AdditionalExtractionLinkValidation(AdditionalExtractionLink parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly AdditionalExtractionLink parent;

		public void ValidateQueryPK()
		{
			ValidateCalculatedProperty(parent.QueryPKInfo);
		}

		protected void CheckQueryPK()
		{
			MandatoryValidation.CheckEntered(parent.QueryPKInfo);
			ListValidation.ErrorIfInvalidPK(parent.QueryPKInfo);
		}

		public void ValidateExtractionPK()
		{
			ValidateCalculatedProperty(parent.ExtractionPKInfo);
		}

		protected void CheckExtractionPK()
		{
			MandatoryValidation.CheckEntered(parent.ExtractionPKInfo);
			ListValidation.ErrorIfInvalidPK(parent.ExtractionPKInfo);

			if (parent.BaseExtraction != null && parent.RelatedExtraction != null && !parent.BaseExtraction.CategoryColumns.Cast<SQLColumnSpecification>().Where(c => c.Selected).OrderBy(c => c.Sequence).SequenceEqual(parent.RelatedExtraction.CategoryColumns.Cast<SQLColumnSpecification>().Where(c => c.Selected).OrderBy(c => c.Sequence), new SQLColumnSpecification.SQLColumnSpecificationComparer()))
			{
				parent.ExtractionPKInfo.AddError(Res.GetString("13cade5b-d391-4d48-bff3-ed299ef25777", "Category Columns must be the same across additional extractions"));
			}
		}

		public override void ValidateAll()
		{
			ValidateExtractionPK();
			ValidateQueryPK();
		}

		public override Type AutoValidationType
		{
			get { return typeof(AdditionalExtractionLinkValidation); }
		}
	}
}
