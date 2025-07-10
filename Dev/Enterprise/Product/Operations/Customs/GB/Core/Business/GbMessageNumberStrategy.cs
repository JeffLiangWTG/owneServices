using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Business
{
	public class GbMessageNumberStrategy : IMessageNumberStrategy
	{
		public GbMessageNumberStrategy(BusinessObjectFactory factory, string applicationCode)
		{
			Argument.NotNull(factory, "factory");
			Argument.NotNull(applicationCode, "applicationCode");
			this.factory = factory;
			this.ApplicationCode = applicationCode;
		}
		readonly BusinessObjectFactory factory;

		#region IMessageNumberStrategy Members

		public string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", this.ApplicationCode).GetNextFormatted(factory);
		}

		#endregion

		public string ApplicationCode
		{
			get;
			private set;
		}
	}

	public class GbInterchangeNumberStrategy : IMessageNumberStrategy
	{
		public GbInterchangeNumberStrategy(BusinessObjectFactory factory, string applicationCode)
		{
			Argument.NotNull(factory, "factory");
			Argument.NotNull(applicationCode, "applicationCode");
			this.factory = factory;
			this.ApplicationCode = applicationCode;
		}
		readonly BusinessObjectFactory factory;

		#region IMessageNumberStrategy Members

		public string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("I", "ENT", this.ApplicationCode).GetNextFormatted(factory);
		}

		#endregion

		public string ApplicationCode
		{
			get;
			private set;
		}
	}
}
