using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.General.Testing
{
	[TestedType(typeof(DocJobComInvoiceHeader))]
	sealed class DocJobComInvoiceHeaderTest : DocBaseJobComInvoiceHeaderAbstractTest<BaseJobComInvoiceHeader, DocJobComInvoiceHeader>
	{
		#region Overrides

		public override void TestIncoTermDescription()
		{
			CustomsIncoTermOverrideCollection collection = DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.Value;
			collection.AddNew("FDD", Core.Constants.IncoTerms.FreeAlongsideShip);
			DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			InvoiceHeaderInternal.JZ_IncoTerm = "EXW";
			Assert("Inco term should be empty", InvoiceHeaderWrapperInternal.IncoTermDescription != ZString.Empty);

			InvoiceHeaderInternal.JZ_IncoTerm = "CIF";
			Assert("Inco term should not be empty", InvoiceHeaderWrapperInternal.IncoTermDescription != ZString.Empty);

			InvoiceHeaderInternal.JZ_IncoTerm = "FDD";
			AssertEquals("IncoTerm Override", Factory.GetCachedValue<IncoTermsCodeDescriptionPairList>().GetDescriptionFromCode(Core.Constants.IncoTerms.FreeAlongsideShip), InvoiceHeaderWrapperInternal.IncoTermDescription);
		}

		#endregion

		#region ZDecimal Fields
		public override void TestConversionFactorIsWrapped()
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = 1000m;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1m, InvoiceHeaderWrapperInternal.ConversionFactor);
		}
		#endregion

		#region Implementation

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Fiji; }
		}

		protected override DocJobComInvoiceHeader CreateInvoiceHeaderWrapper(BaseJobComInvoiceHeader invoiceHeaderInternal)
		{
			return DocJobComInvoiceHeader.New(invoiceHeaderInternal, Factory);
		}

		#endregion
	}
}
