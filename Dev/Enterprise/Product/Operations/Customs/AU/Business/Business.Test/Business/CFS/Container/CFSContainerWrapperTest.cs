using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CFSContainerWrapper))]
	public class CFSContainerWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUsesTranshipmentPortOnUnderbond()
		{
			AssertEquals(false, ((ICusUnderbondDependentCollectionParent)Wrapper).UsesTranshipmentPortOnUnderbond);
			AssertEquals(ZString.Empty, ((ICusUnderbondDependentCollectionParent)Wrapper).DefaultTranshipmentPort);
		}

		public void TestIsForAirCargo()
		{
			AssertEquals(false, ((ICusUnderbondUnionCollectionParent)Wrapper).IsForAirCargo);
		}

		public void TestUnderbondHumanReadableName()
		{
			Container.JC_ContainerNum = "12345";
			AssertEquals("UnderbondHumanReadableName", "Container: 12345", Wrapper.UnderbondHumanReadableName);
		}

		public void TestUnderbonds()
		{
			AssertNotNull("Underbonds", Wrapper.Underbonds);
		}

		public void TestIOutturnableLine_IsDeleted()
		{
			AssertEquals("IsDeleted", false, ((IOutturnableLine)Wrapper).IsDeleted);
			Container.Delete();
			AssertEquals("IsDeleted", true, ((IOutturnableLine)Wrapper).IsDeleted);
		}

		public void TestIOutturnableLine_TableName()
		{
			AssertEquals("TableName", Enterprise.ZArchitecture.Schema.JobContainerSchema.Constants.TableName, ((ILinkable)Wrapper).LinkTableName);
		}

		public void TestIOutturnableLine_PK()
		{
			AssertEquals("PK", Container.PK, ((ILinkable)Wrapper).LinkPK);
		}

		public void TestICusUnderbondDependentCollectionParent_Details()
		{
			Container.JC_ContainerNum = "12345";
			AssertEquals("Details", "Container: 12345", ((ICusUnderbondDependentCollectionParent)Wrapper).Details);
		}

		public void TestGetAllPossibleCollectionProviders()
		{
			CFSContainer container = Factory.New<CFSContainer>();

			CFSPackLine packLine1 = Factory.New<CFSPackLine>();
			packLine1.JL_FreightMode = FreightConstants.OuterPackType;
			container.PackLines.Add(packLine1);

			CFSPackLine packLine2 = Factory.New<CFSPackLine>();
			packLine2.JL_FreightMode = FreightConstants.OuterPackType;

			CFSShipment shipment = Factory.New<CFSShipment>();
			packLine2.JL_JS = shipment.PK;
			container.PackLines.Add(packLine2);

			CFSContainerWrapper wrapper = CFSContainerWrapper.Load(container);
			ICusUnderbondDependentCollectionParent[] providers = wrapper.GetAllPossibleCollectionProviders();
			AssertEquals(2, providers.Length);
			AssertEquals(shipment, ((CFSShipmentWrapper)providers[0]).Shipment);
		}

		public void TestIMessageManageableBizObj()
		{
			CFSContainer container = Factory.New<CFSContainer>();

			CFSPackLine packLine1 = Factory.New<CFSPackLine>();
			packLine1.JL_FreightMode = FreightConstants.OuterPackType;
			container.PackLines.Add(packLine1);

			CFSPackLine packLine2 = Factory.New<CFSPackLine>();
			packLine2.JL_FreightMode = FreightConstants.OuterPackType;

			CFSShipment shipment = Factory.New<CFSShipment>();
			packLine2.JL_JS = shipment.PK;
			container.PackLines.Add(packLine2);

			CFSContainerWrapper wrapper = CFSContainerWrapper.Load(container);

			Customs.Business.IMessageManageableBizObj bizObj = wrapper;

			AssertEquals("MessageManager", typeof(SeaCargoDepotMultiMessageManager), bizObj.GetMessageManagerForAmendmentDetection().GetType());
		}

		public void TestCanSendWithoutDelay()
		{
			Assert(((ICusUnderbondDependentCollectionParent)Wrapper).CanSendWithoutDelay);
		}

		public void TestICusUnderbondDependentCollectionParent_PK()
		{
			AssertEquals("PK", Container.PK, ((ICusUnderbondDependentCollectionParent)Wrapper).LinkPK);
		}

		public void TestICusUnderbondDependentCollectionParent_OutturnableLines()
		{
			var shipment = Factory.New<CFSShipment>();
			Container.PackUnpackShipments.Add(shipment);
			IOutturnableLine[] lines = ((ICusUnderbondDependentCollectionParent)Wrapper).OutturnableLines;
			AssertEquals("Length", 2, lines.Length);
			//TODO: fix this
			AssertEquals("Lines[0]", typeof(CFSContainerWrapper), lines[0].GetType());
			AssertEquals("Lines[1]", typeof(CFSShipmentWrapper), lines[1].GetType());
		}

		public void TestIOutturnableLinePackagesManifested()
		{
			CFSPackLine packLine = Factory.New<CFSPackLine>();
			packLine.JL_PackageCount = 8;
			Container.PackLines.Add(packLine);
			AssertEquals(8, ((IOutturnableLine)Wrapper).PackagesManifested);
		}

		public void TestCorretType()
		{
			AssertType<CusUnderbondUnionCollection>(Wrapper.AllUnderbonds);
			AssertType<CusUnderbondCollection>(Wrapper.Underbonds);
		}

		#region Implementation

		CFSContainerWrapper fWrapper;
		CFSContainerWrapper Wrapper
		{
			get
			{
				if (fWrapper == null)
				{
					fWrapper = (CFSContainerWrapper)GetNewBusinessObject();
				}
				return fWrapper;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CFSContainerWrapper.Load(Container);
		}

		CFSContainer fContainer;
		CFSContainer Container
		{
			get
			{
				if (fContainer == null)
				{
					fContainer = Factory.New<CFSContainer>();
				}
				return fContainer;
			}
		}

		#endregion
	}
}
