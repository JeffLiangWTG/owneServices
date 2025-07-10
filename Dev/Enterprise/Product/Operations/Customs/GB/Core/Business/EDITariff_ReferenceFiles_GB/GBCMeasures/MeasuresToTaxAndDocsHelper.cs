using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business
{
	// Provides tax and docs details to an ITaxAndDocsProvider, e.g. an Invoiceline or a Pivot
	public static class MeasuresToTaxAndDocsHelper
	{
		#region Implementation

		// This odd pattern to make Enterprise.ReflectionTest.ReflectionTestHelper.TestStaticMethodsAreLocatedOnCorrectClass() STFU
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public static void FindExistingSupportingDocument(string code, EU.Business.Declaration.MultiLineAddInfos.ISupportingDocumentsProvider provider, out SupportingDocument suppDoc)
		{
			foreach (SupportingDocument sd in provider.SupportingDocuments)
			{
				if (sd.CSI_Code == code)
				{
					suppDoc = sd;
					return;
				}
			}
			suppDoc = null;
		}

		public static void FindExistingSupportingDocument(string code, EU.Business.Declaration.MultiLineAddInfos.ISupportingDocumentsProvider provider, ZString invoiceNum, out SupportingDocument suppDoc)
		{
			foreach (SupportingDocument sd in provider.SupportingDocuments)
			{
				if (sd.CSI_Code == code && sd.CSI_ReferenceNumber == invoiceNum)
				{
					suppDoc = sd;
					return;
				}
			}
			suppDoc = null;
		}

		internal static void FindExistingOrAddNewSupportingDocument(string code, EU.Business.Declaration.MultiLineAddInfos.ISupportingDocumentsProvider provider, out SupportingDocument sd)
		{
			FindExistingSupportingDocument(code, provider, out sd);
			if (sd == null)
			{
				sd = (SupportingDocument)provider.SupportingDocuments.AddNew();
				sd.CSI_Code = code;
			}
		}
		#endregion
	}
}
