using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWithholdingTaxRate : DocumentWrapper
	{
		DocWithholdingTaxRate(AccWithholding accWithholding, BusinessObjectFactory factoryToWrap)
			: base(accWithholding, factoryToWrap)
		{
		}

		public static DocWithholdingTaxRate New(AccWithholding accWithholding, BusinessObjectFactory factoryToWrap)
		{
			if (accWithholding == null)
			{
				return null;
			}
			else
			{
				return new DocWithholdingTaxRate(accWithholding, factoryToWrap);
			}
		}

		AccWithholding AccWithholding
		{
			get { return (AccWithholding)WrappedObject; }
		}

		public override string ToString()
		{
			return NoDefaultPropertyErrorMessage;
		}

		public ZString Code
		{
			get { return AccWithholding.AW_Code; }
		}

		public ZString Description
		{
			get { return AccWithholding.AW_Description; }
		}

		public DocCompany Company
		{
			get { return DocCompany.New(AccWithholding.Company, Factory); }
		}

		public ZBool IsActive
		{
			get { return AccWithholding.AW_IsActive; }
		}

		public ZDecimal Rate
		{
			get { return AccWithholding.AW_Rate; }
		}
	}
}
