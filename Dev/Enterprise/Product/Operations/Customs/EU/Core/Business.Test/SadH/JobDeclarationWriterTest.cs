using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using CusEntryInstruction = Enterprise.Customs.Business.CusEntryInstruction;
using JobDeclarationWriter = Enterprise.Customs.EU.Business.SADH.JobDeclarationWriter;
using SADHFormData = Enterprise.Customs.EU.Business.SADH.SADHFormData;

namespace Enterprise.Customs.EU.Business.Testing
{
	class JobDeclarationWriterTest : Customs.Business.SADH.Testing.JobDeclarationWriterTest
	{
		#region Declaration
		protected override BaseJobDeclaration GetNewJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		protected new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}
		#endregion

		#region FormData
		protected override Customs.Business.SADH.SADHFormData GetNewSADHFormData()
		{
			return new SADHFormData(Factory, Declaration);
		}

		protected new SADHFormData FormData
		{
			get { return (SADHFormData)base.FormData; }
		}
		#endregion

		#region Writer
		protected override Customs.Business.SADH.JobDeclarationWriter GetNewJobDeclarationWriter()
		{
			return new JobDeclarationWriter(Declaration);
		}

		protected new JobDeclarationWriter Writer
		{
			get { return (JobDeclarationWriter)base.Writer; }
		}
		#endregion

		protected override void SetupFormDataFields(RefCurrency currency, GlbBranch branch)
		{
			base.SetupFormDataFields(currency, branch);
			SADHFormData formData = FormData;
			formData.D1_CommodityCode = "2010421010";
			formData.D1_EntryType = "XY";
			formData.D1_EntrySubType = "Z";
			formData.D1_RepresentationType = "DIR";
			formData.D1_InlandModeOfTransport = "13";
			formData.D1_DepartureTransportID = "TR123";
		}

		protected override void AssertWriteFromSavesAllFieldsBackToDeclaration(RefCurrency currency, GlbBranch branch, BaseJobComInvoiceHeader firstInvoice, BaseJobComInvoiceLine firstLine)
		{
			base.AssertWriteFromSavesAllFieldsBackToDeclaration(currency, branch, firstInvoice, firstLine);
			JobDeclaration declaration = Declaration;
			var cei = declaration.CustomsEntryInstructions.FirstOrDefault<CusEntryInstruction>();
			AssertEquals("firstLine.JI_Tariff", "2010421010", firstLine.JI_Tariff);
			AssertEquals("declaration.JE_EntryStyle", "XY", declaration.JE_EntryStyle);
			AssertEquals("declaration.JE_EntrySubStyle", "Z", cei.CEI_SubStyle);
			AssertEquals("declaration.JE_DeclarantType", "DIR", declaration.JE_DeclarantType);
			AssertEquals("declaration.JE_InlandModeOfTransport", "13", declaration.JE_TransportModeInland);
			AssertEquals("declaration.JE_VesselName", "USS ENTERPRISE", declaration.JE_VesselName);

			FormData.D1_ModeOfTransportAtTheBorder = "RAI";
			Writer.WriteFrom(FormData);
			AssertEquals("declaration.JE_VesselName", "TR123", declaration.JE_VesselName);
		}
	}
}
