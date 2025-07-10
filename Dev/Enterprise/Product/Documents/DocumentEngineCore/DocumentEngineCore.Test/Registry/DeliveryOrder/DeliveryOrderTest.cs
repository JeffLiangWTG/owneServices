using System;
using System.Drawing;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DeliveryOrder))]
	public class DeliveryOrderTest : RegistryBusinessObjectTemplateTestCase
	{
		#region TestRowValidation

		public void TestRowValidation()
		{
			const string error1 = "Please select an Image.";
			const string error2 = "This image is 5.1MB, this is way to big. you should be able to get a full page image of text down to around 500KB.";
			const string warning = "This image is 1.1MB, this is a bit excessive. you should be able to get a full page image of text down to around 500KB.";

			DeliveryOrderCollection collection = new DeliveryOrderCollection(NewFallBackLevel(), Factory);
			DeliveryOrder dO = collection.AddNew();

			dO.PrincipalPK = Principal.PK;
			dO.PrintParameter = DeliveryOrder.PrintConstants.Code.PDO;

			dO.ValidateRow();
			AssertHasRowError(dO, error1);

			dO.Image = new Bitmap(10, 10);
			AssertNoRowError(dO, error1);

			typeof(DeliveryOrder).InvokeMember("imageSize", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, dO, new object[] { (long?)(1.1 * 1024 * 1024) });
			dO.ValidateRow();
			AssertHasRowWarning(dO, warning);

			typeof(DeliveryOrder).InvokeMember("imageSize", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, dO, new object[] { (long?)(5.1 * 1024 * 1024) });
			dO.ValidateRow();
			AssertHasRowError(dO, error2);
		}

		#endregion

		#region TestPrintParameterValidation

		public void TestPrintParameterValidation()
		{
			DeliveryOrderCollection collection = new DeliveryOrderCollection(NewFallBackLevel(), Factory);
			DeliveryOrder dO = collection.AddNew();

			dO.PrintParameter = "";
			AssertHasError(dO.PrintParameterInfo, "Please enter a value.");

			dO.PrintParameter = "Bla";
			AssertHasError(dO.PrintParameterInfo, "Enter a valid print parameter.");

			dO.PrintParameter = DeliveryOrder.PrintConstants.Code.PDO;
			AssertNoErrors(dO.PrintParameterInfo);
		}

		#endregion

		#region TestPrincipalValidation

		const string NoPrincipal = "Please enter a Principal.";
		const string InvalidPrincipal = "Enter a valid principal.";
		const string DuplicatePrincipal = "A principal may only appear once in this list.";

		public void TestPrincipalValidation()
		{
			AssertNotNull("lazy load", Principal);

			DeliveryOrderCollection collection = new DeliveryOrderCollection(NewFallBackLevel(), Factory);

			DeliveryOrder dO = collection.AddNew();
			dO.PrincipalPK = ZGuid.Empty;
			dO.ValidatePrincipalPK();
			AssertHasError(dO.PrincipalPKInfo, NoPrincipal);

			dO.PrincipalPK = ZGuid.NewZGuid();
			AssertNoError(dO.PrincipalPKInfo, NoPrincipal);
			AssertHasError(dO.PrincipalPKInfo, InvalidPrincipal);

			dO.PrincipalPK = Principal.PK;
			AssertNoNotifications(dO.PrincipalPKInfo);

			DeliveryOrder dO1 = collection.AddNew();
			dO1.PrincipalPK = Principal.PK;
			AssertHasError(dO1.PrincipalPKInfo, DuplicatePrincipal);
		}

		#endregion

		#region Implementation

		#region Principal

		BusinessObject Principal
		{
			get
			{
				if (principal == null)
				{
					principal = CreatePrincipal("Principal1");
				}

				return principal;
			}
		}

		BusinessObject principal;

		BusinessObject CreatePrincipal(ZString code)
		{
			BusinessObjectFactory dirtyFactory = new BusinessObjectFactory();
			BusinessObject principal = dirtyFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			principal[OrgHeaderSchema.Constants.OH_Code] = code;
			principal[OrgHeaderSchema.Constants.OH_IsShippingProvider] = true;

			BusinessObject companyData = (BusinessObject)principal["CompanyData"];
			companyData[OrgCompanyDataSchema.Constants.OB_CRIsShipsAgencyPrincipal] = true;
			dirtyFactory.Save();

			return (BusinessObject)Factory.Load<IOrgHeader>(principal.PK);
		}

		#endregion

		FallbackLevel NewFallBackLevel()
		{
			return new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			DeliveryOrderCollection collection = new DeliveryOrderCollection(NewFallBackLevel(), Factory);
			DeliveryOrder dO = collection.AddNew();

			return dO;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);

			DeliveryOrder originalDO = (DeliveryOrder)originalBusinessObject;
			DeliveryOrder newDO = (DeliveryOrder)newBusinessObject;

			AssertEquals("PrincipalPK", originalDO.PrincipalPK, newDO.PrincipalPK);
			AssertEquals("PrincipalPK", originalDO.PrintParameter, newDO.PrintParameter);
			AssertEquals("Image", true, Utilities.IsImageEqual(originalDO.Image, newDO.Image));
		}

		DeliveryOrder GetNewPopulatedBusinessObject()
		{
			DeliveryOrderCollection collection = new DeliveryOrderCollection(NewFallBackLevel(), Factory);
			DeliveryOrder dO = collection.AddNew();
			dO.PrincipalPK = Principal.PK;
			dO.PrintParameter = "DPO";
			dO.Image = new Bitmap(10, 10);

			return dO;
		}

		#endregion
	}
}
