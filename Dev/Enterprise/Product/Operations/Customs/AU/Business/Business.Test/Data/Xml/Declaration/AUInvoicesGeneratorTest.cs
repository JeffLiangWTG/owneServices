using System;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUInvoicesGeneratorTest : InvoicesGeneratorFromXSDTest
	{
		protected override Type ExpectedInvoiceAdapterType
		{
			get
			{
				return typeof(AUInvoiceValueObjectDataAdapter);
			}
		}

		protected override InvoicesGeneratorFromXSD Generator
		{
			get
			{
				if (fGenerator == null)
				{
					fGenerator = new AUInvoicesGeneratorFromXSD(declaration);
				}

				return fGenerator;
			}
		}

		AUInvoicesGeneratorFromXSD fGenerator;
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
		}
	}
}
