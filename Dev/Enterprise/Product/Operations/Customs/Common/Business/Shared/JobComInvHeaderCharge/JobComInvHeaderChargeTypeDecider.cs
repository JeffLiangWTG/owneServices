using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common
{
	public class JobComInvHeaderChargeTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var tableCode = (string)row[JobComInvHeaderChargeSchema.J7_ParentTableCode.Name];

			switch (tableCode)
			{
				case JobComInvoiceHeaderSchema.Constants.Prefix:
				case JobComInvoiceLineSchema.Constants.Prefix:
					return ObjectFactory.Get<Integration.Customs.IBaseJobComInvHeaderChargeTypeDecider>().GetTypeForLoad(row, factory);

				case JobOrderHeaderSchema.Constants.Prefix:
				case JobOrderLineSchema.Constants.Prefix:
					return ObjectFactory.GetType<Integration.Freight.IJobComInvHeaderCharge>();
#if DEBUG
				case CargoWise.EntityFramework.Testing.DummyBaseBusinessObject.Schema.TablePrefix:
				case "Z1":

					bool isApportioned = (bool)row[JobComInvHeaderChargeSchema.J7_IsApportionedCharge.Name];

					return isApportioned ? typeof(Testing.TestApportionedCharge) : typeof(Testing.TestCharge);
#endif
				default:
					return null;
			}
		}

		public override Type GetTypeForBinding()
		{
			throw new NotImplementedException();
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
