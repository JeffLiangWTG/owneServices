using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.ApplicationLogging.Business
{
	public class ApplicationActiveLogger : AutoApplicationActiveLogger
	{
		public ApplicationActiveLogger(
			BusinessObjectFactory factory,
			DataRow row)
			: base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;
		protected override ZString HumanReadableNameCore => Res.GetString("4BDB69B6-D00A-4067-9CA1-C88BD4506DEC", "Application Active Logger");

		[List("Lookups.ApplicationLoggers")]
		[RelatedBusinessObject("ApplicationLogger")]
		[ReadOnlyMember(nameof(IsInDatabase))]
		public override ZGuid AAL_ALG_ApplicationLogger
		{
			get => base.AAL_ALG_ApplicationLogger;
			set => base.AAL_ALG_ApplicationLogger = value;
		}

		public ZString ApplicationLoggerProduct => ApplicationLogger == null ? ZString.Empty : ApplicationLogger.ALG_Product;

		public ZString ApplicationLoggerName => ApplicationLogger == null ? ZString.Empty : ApplicationLogger.ALG_Name;

		public ApplicationLogger ApplicationLogger
		{
			get
			{
				if (applicationLogger == null)
				{
					applicationLogger = Factory.Load<ApplicationLogger>(AAL_ALG_ApplicationLogger);
				}
				return applicationLogger;
			}
		}

		[List("Lookups.Licences")]
		[RelatedBusinessObject("Licence")]
		[ReadOnlyMember(nameof(IsInDatabase))]
		public ZGuid LicenceGuid
		{
			get
			{
				return licence?.PK ?? ZGuid.Empty;
			}
			set
			{
				licence = Factory.Load<LicenceHeader>(value);
				AAL_Environment = licence is not null
					? $"{licence.Database.EnterpriseCode}{licence.Database.LD_ServerCode}"
					: string.Empty;
			}
		}

		LicenceHeader licence;
		ApplicationLogger applicationLogger;
	}
}
