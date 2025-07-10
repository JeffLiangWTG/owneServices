using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.SADH;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing
{
	class SADHFormDataReaderTest : Customs.Business.SADH.Testing.SADHFormDataReaderTest
	{
		protected override BaseJobDeclaration GetNewJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		protected new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected override Customs.Business.SADH.SADHFormData GetNewSADHFormData()
		{
			return new SADHFormData(Factory, Declaration);
		}

		protected new SADHFormData FormData
		{
			get { return (SADHFormData)base.FormData; }
		}

		protected override Customs.Business.SADH.SADHFormDataReader GetNewSADHFormDataReader()
		{
			return new SADHFormDataReader(FormData);
		}

		protected new SADHFormDataReader Reader
		{
			get { return (SADHFormDataReader)base.Reader; }
		}

		protected override BaseJobComInvoiceHeader CreateNewInoviceForTest()
		{
			return Factory.New<InvoiceHeaderForTest>();
		}

		protected override void SetupDeclarationFields(OrgHeader consignor, OrgHeader consignee, GlbBranch branch)
		{
			base.SetupDeclarationFields(consignor, consignee, branch);
			var declaration = Declaration;
			declaration.JE_EntryStyle = "XY";
			declaration.JE_DeclarantType = "DIR";
			declaration.JE_TransportModeInland = "13";
			declaration.JE_LocationOfGoods = new ZString('A', declaration.JE_LocationOfGoodsInfo.MaxLength);
		}

		protected override void AssertReadFromFillsInAllFieldsFromDeclaration(OrgHeader consignor, OrgHeader consignee, GlbBranch branch, RefCurrency currency)
		{
			base.AssertReadFromFillsInAllFieldsFromDeclaration(consignor, consignee, branch, currency);
			SADHFormData formData = FormData;
			AssertEquals("formData.D1_EntryType", "XY", formData.D1_EntryType);

			AssertEquals("formData.D1_RepresentationType", "DIR", formData.D1_RepresentationType);
			AssertEquals("formData.D1_InlandModeOfTransport", "13", formData.D1_InlandModeOfTransport);
			AssertEquals("formData.D1_RL_NKLocationOfGoods", new ZString('A', Declaration.JE_LocationOfGoodsInfo.MaxLength), formData.D1_LocationOfGoods);
			AssertEquals("formData.D1_DepartureTransportID", "", formData.D1_DepartureTransportID);

			Declaration.JE_TransportMode = "RAI";
			Declaration.JE_VesselName = "USS ENTERPRISE";
			Reader.ReadFrom(Declaration);
			AssertEquals("formData.D1_DepartureTransportID", "USS ENTERPRISE", formData.D1_DepartureTransportID);
		}

		class InvoiceHeaderForTest : JobComInvoiceHeader
		{
			public InvoiceHeaderForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			protected override ZDateTime EffectiveValuationDateCore
			{
				get
				{
					var originalEntryInstruction = InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.EntryInstruction != null).Select(x => x.EntryInstruction)
						.Distinct().OrderBy(x => x.CEI_SubStyle).ThenBy(x => x.CEI_Description).FirstOrDefault(x => x.CEI_DateForDuty.IsValid);
					return originalEntryInstruction?.CEI_DateForDuty ?? base.EffectiveValuationDateCore;
				}
			}
		}
	}
}
