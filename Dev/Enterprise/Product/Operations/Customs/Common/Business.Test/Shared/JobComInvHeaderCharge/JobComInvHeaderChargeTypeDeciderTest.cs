using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing
{
	sealed class JobComInvHeaderChargeTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			var typeDecider = new JobComInvHeaderChargeTypeDecider();
			AssertExceptionThrown<NotImplementedException>(() => typeDecider.GetTypeForBinding());
		}

		[ExpectNoExceptions]
		public void TestGetTypeForNew()
		{
			var typeDecider = new JobComInvHeaderChargeTypeDecider();
			NUnit.Framework.Assert.That(typeDecider.GetTypeForNew(), Is.EqualTo(default(Type)));
		}

		[ExpectNoExceptions]
		public void TestGetTypeForLoad_JZ_JI()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "CNZ";
			nzCompany.GC_Name = "NZ Company";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "BNZ";

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
			declaration.JE_GB = nzBranch.PK;
			var invoice1 = declaration.Invoices.AddNew();
			var invoice1Charge = invoice1.Charges.AddNew();
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			var inv1Line1Charge = invoice1Line1.Charges.AddNew();
			Factory.Save();

			CombineAssertions(() =>
			{
				var typeDecider = new JobComInvHeaderChargeTypeDecider();
				NUnit.Framework.Assert.That(typeDecider.GetTypeForLoad(((IBusinessObjectInternals)invoice1Charge).Row, Factory).FullName, Is.EqualTo("Enterprise.Customs.NZ.Business.Declaration.InvoiceCharge"), "JZ");
				NUnit.Framework.Assert.That(typeDecider.GetTypeForLoad(((IBusinessObjectInternals)inv1Line1Charge).Row, Factory).FullName, Is.EqualTo("Enterprise.Customs.NZ.Business.Declaration.InvoiceLineCharge"), "JI");
			});
		}

		[ExpectNoExceptions]
		public void TestGetTypeForLoad_JD_JO()
		{
			var order = Factory.New(ObjectFactory.GetType<Integration.Forwarding.IOrder>());
			var orderLine = Factory.New(ObjectFactory.GetType<Integration.Forwarding.IOrderLine>());

			var charge = Factory.New(ObjectFactory.GetType<Integration.Freight.IJobComInvHeaderCharge>());
			charge[JobComInvHeaderChargeSchema.J7_ParentID] = order.PK;
			charge[JobComInvHeaderChargeSchema.J7_ParentTableCode] = JobOrderHeaderSchema.Constants.Prefix;

			var charge2 = Factory.New(ObjectFactory.GetType<Integration.Freight.IJobComInvHeaderCharge>());
			charge2[JobComInvHeaderChargeSchema.J7_ParentID] = orderLine.PK;
			charge2[JobComInvHeaderChargeSchema.J7_ParentTableCode] = JobOrderLineSchema.Constants.Prefix;

			CombineAssertions(() =>
			{
				var typeDecider = new JobComInvHeaderChargeTypeDecider();
				NUnit.Framework.Assert.That(typeDecider.GetTypeForLoad(((IBusinessObjectInternals)charge).Row, Factory).FullName, Is.EqualTo("Enterprise.Freight.Forwarding.Orders.Business.JobComInvCharge"), "JD");
				NUnit.Framework.Assert.That(typeDecider.GetTypeForLoad(((IBusinessObjectInternals)charge2).Row, Factory).FullName, Is.EqualTo("Enterprise.Freight.Forwarding.Orders.Business.JobComInvCharge"), "JO");
			});
		}
	}
}
