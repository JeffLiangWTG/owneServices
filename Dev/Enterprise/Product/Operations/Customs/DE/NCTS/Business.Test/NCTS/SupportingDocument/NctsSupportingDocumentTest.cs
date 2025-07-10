using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsSupportingDocument))]
	sealed class NctsSupportingDocumentTest : CusSupportingInfoTest<NctsSupportingDocument>
	{
		public void TestValidation_Arrival()
		{
			AssertType<CusSupportingInfoDisabledValidation>(supportingDocument.Validation);
		}

		public void TestValidation_Departure()
		{
			AssertType<NctsSupportingDocumentDepartureValidation>(departureSupportingDocument.Validation);
		}

		protected override IEnumerable<NctsSupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = nctsHeader.Bills.AddNew();
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			yield return (NctsSupportingDocument)nctsHeader.ArrivalMovementHeader.SupportingDocuments.AddNew();
			yield return goodsItem.SupportingDocuments.AddNew();
			yield return (NctsSupportingDocument)bill.SupportingDocuments.AddNew();
		}

		protected override void LoadParentIfNeeded(BusinessObjectFactory factory, NctsSupportingDocument bizObj)
		{
			base.LoadParentIfNeeded(factory, bizObj);
			if (bizObj.Parent is NctsHeader _)
			{
				factory.Load<NctsHeader>(bizObj.CSI_ParentID);
			}
			else if (bizObj.Parent is NctsArrivalMovementHeader _)
			{
				factory.Load<NctsArrivalMovementHeader>(bizObj.CSI_ParentID);
			}
			else if (bizObj.Parent is NctsBill _)
			{
				factory.Load<NctsBill>(bizObj.CSI_ParentID);
			}
			else if (bizObj.Parent is NctsCommonCargoDesc _)
			{
				factory.Load<NctsCommonCargoDesc>(bizObj.CSI_ParentID);
			}
		}

		protected override BusinessObject GetNewBusinessObject() => supportingDocument;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = nctsHeader.Bills.AddNew();
			supportingDocument = (NctsSupportingDocument)bill.SupportingDocuments.AddNew();
		}
		NctsSupportingDocument supportingDocument;
		NctsHeader nctsHeader;

		NctsSupportingDocument departureSupportingDocument
		{
			get
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var bill = nctsHeader.Bills.AddNew();
				return (NctsSupportingDocument)bill.SupportingDocuments.AddNew();
			}
		}
	}
}
