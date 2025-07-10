using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRMessageNumberStrategy : IMessageNumberStrategy
	{
		public FRMessageNumberStrategy(BusinessObjectFactory factory, string applicationCode)
		{
			Argument.NotNull(factory, "factory");
			Argument.NotNull(applicationCode, "applicationCode");
			this.factory = factory;
			this.ApplicationCode = applicationCode;
		}
		readonly BusinessObjectFactory factory;
		public string ApplicationCode { get; private set; }

		public string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", this.ApplicationCode).GetNextFormatted(factory);
		}
	}

	[CodeAlive("Used in future CIN WI")]
	public class CINMessageNumberStrategy : IMessageNumberStrategy
	{
		public CINMessageNumberStrategy(BusinessObjectFactory factory, string applicationCode)
		{
			Argument.NotNull(factory, "factory");
			Argument.NotNull(applicationCode, "applicationCode");

			this.factory = factory;
			this.ApplicationCode = applicationCode;
		}

		#region IMessageNumberStrategy Members
		public string GetMessageReferenceNumber()
		{
			var fountain = Env.NumberFountains.GetCINSendCounter();
			var nextNumber = (int)fountain.GetNext(factory);

			return nextNumber.ToString(CultureInfo.CurrentCulture).PadLeft(5, '0');
		}

		#endregion
		public string ApplicationCode
		{
			get;
			private set;
		}

		readonly BusinessObjectFactory factory;
	}
}
