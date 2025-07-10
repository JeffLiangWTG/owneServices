using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.MatchProduct;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgSupplierPartMatchings
{
	public class OrgSupplierPartMatchingInterceptorTest : TransactionedTestCase
	{
		public void TestInvoke()
		{
			var setting = new OrgSupplierPartMatchingSetting();
			setting.Context = context;

			var interceptor = new OrgSupplierPartMatchingInterceptor(setting, sessionServices);
			interceptor.Function = e => { }; // Dummy Method
			interceptor.Helper = helper.Object;

			var partEntity = new Entity(TestUtil.FindEntityDefinition("Order", "JobOrderHeader.LandedCostHeader.LandedCostHistory.OrgSupplierPart"), sessionServices);

			//Setup part in Db
			var supplier = factory.NewWithValidTestData<OrgHeader>();
			var buyer = factory.NewWithValidTestData<OrgHeader>();
			var part = factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "Test";
			part.RelatedOrganisations.AddSupplier(supplier);
			part.RelatedOrganisations.AddOwner(buyer);
			factory.Save();

			helper.Setup(m => m.FindBuyer(Moq.It.IsAny<IEntity>())).Returns(buyer);
			helper.Setup(m => m.FindSupplier(Moq.It.IsAny<IEntity>())).Returns(supplier);
			helper.Setup(m => m.FindSupplierPartEntities(Moq.It.IsAny<IEntity>())).Returns(new[] { partEntity });
			helper.Setup(m => m.ConvertPartEntityToPart(Moq.It.IsAny<IEntity>(),Moq.It.IsAny<IOrgHeader>(),Moq.It.IsAny<IOrgHeader>())).Returns(new Part
			{
				PartNum = "Test",
				Supplier = supplier,
				Buyer = buyer
			});

			interceptor.Invoke(new EntitySet("Order"));
			AssertEquals(partEntity.InternalPK, part.PK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			sessionServices = new AncillaryImportServices();
			context = new EntityContext(sessionServices, new FactoryProvider());
			factory = context.ObjectFactory;
			helper = new Mock<IOrgSupplierPartMatchingHelper>();
		}
		AncillaryImportServices sessionServices;
		Mock<IOrgSupplierPartMatchingHelper> helper;
		EntityContext context;
		BusinessObjectFactory factory;
	}
}
