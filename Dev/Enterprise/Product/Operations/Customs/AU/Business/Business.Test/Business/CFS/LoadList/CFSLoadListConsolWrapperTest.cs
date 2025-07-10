using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CFSLoadListConsolWrapper))]
	public class CFSLoadListConsolWrapperTest : SeaCargoDepotNonPersistantBusineesObjectTestCase
	{
		public void TestIMessageManageableBizObj()
		{
			Customs.Business.IMessageManageableBizObj bizObj = Wrapper;

			AssertEquals("MessageManager", typeof(SeaCargoDepotMultiMessageManager), bizObj.GetMessageManagerForAmendmentDetection().GetType());
		}

		public void TestIsForAirCargo()
		{
			AssertEquals(false, ((ICusUnderbondUnionCollectionParent)Wrapper).IsForAirCargo);
		}

		public void TestIOutturnableLine_IsDeleted()
		{
			AssertEquals(false, ((IOutturnableLine)Wrapper).IsDeleted);
			LoadListConsol.Delete();
			AssertEquals(true, ((IOutturnableLine)Wrapper).IsDeleted);
		}

		public void TestIOutturnableLine_TableName()
		{
			AssertEquals("TableName", JobConsolSchema.Constants.TableName, ((ILinkable)Wrapper).LinkTableName);
		}

		public void TestIOutturnableLine_PK()
		{
			AssertEquals("PK", LoadListConsol.PK, ((ILinkable)Wrapper).LinkPK);
		}

		public void TestIOutturnableLine_UnderbondHumanReadableName()
		{
			LoadListConsol.JK_MasterBillNum = "123";
			AssertEquals("UnderbondHumanReadableName", "Consol: 123", ((IOutturnableLine)Wrapper).UnderbondHumanReadableName);
		}

		#region Implementation

		CFSLoadListConsolWrapper fWrapper;
		CFSLoadListConsolWrapper Wrapper
		{
			get
			{
				if (fWrapper == null)
				{
					fWrapper = (CFSLoadListConsolWrapper)GetNewBusinessObject();
				}
				return fWrapper;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CFSLoadListConsolWrapper.Load(LoadListConsol);
		}

		CFSLoadListConsol fLoadListConsol;
		CFSLoadListConsol LoadListConsol
		{
			get
			{
				if (fLoadListConsol == null)
				{
					fLoadListConsol = Factory.New<CFSLoadListConsol>();
				}
				return fLoadListConsol;
			}
		}

		#endregion
	}
}
