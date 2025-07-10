using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageDirector
	{
		readonly INotifications notifier;
		readonly EDIInterchange interchange;

#if DEBUG
		public
#endif
		EHubMessageDirector(EDIInterchange interchange, INotifications notifier)
		{
			this.notifier = notifier;
			this.interchange = interchange;
		}

		public static IeHubMessage CreateMessage(EDIInterchange interchange, INotifications notifier)
		{
			return new EHubMessageDirector(interchange, notifier).CreateMessage();
		}

#if DEBUG
		public
#endif
		IeHubMessage CreateMessage()
		{
			try
			{
				var builder = CreateBuilder();
				if (builder == null)
				{
					var eHubErrorMessage = Res.GetString("6c744636-15b1-4999-8e6b-892e6cc59520", "EDI Interchange Application Code '{0}' is not supported.", interchange.EI_ApplicationCode);
					interchange.AddEHubError(eHubErrorMessage);
					notifier.AddEHubError(eHubErrorMessage);
					return null;
				}

				return builder.Build();
			}
			catch (EHubMessageBuilderInvalidInterchangeException e)
			{
				notifier.AddEHubError(e.Message);
				return null;
			}
			catch (Exception e) when (!e.IsCriticalException() && !e.IsOutOfDiskSpaceException() && !e.IsUnableToCreateTempFileException())
			{
				var interchangeNumber = interchange.EI_InterchangeNum;
				var error = Res.GetString("3d8b7931-a2f0-4814-a15f-f842371e0c4e", "Interchange Number: {0}. eHub Id: {1}. Diagnostic Details: {2}", interchangeNumber, interchange.eHubID, interchange.DiagnosticDetails);
				ErrorReporter.ReportOnce(error, e);
				return null;
			}
		}

#if DEBUG
		public virtual
#endif
		EHubMessageBuilder CreateBuilder()
		{
			switch (interchange.EI_ApplicationCode)
			{
				case ApplicationCodeList.Codes.XMS:
					return new EHubMessageBuilderForXMS(interchange, notifier);

				case ApplicationCodeList.Codes.SYS:
					return new EHubMessageBuilderForSYS(interchange, notifier);

				case ApplicationCodeList.Codes.CustomsWare:
				case ApplicationCodeList.Codes.ChinaInterfaceMapping:
				case ApplicationCodeList.Codes.USeBond:
				case ApplicationCodeList.Codes.AirCargoAdvanceScreening:
					return new EHubMessageBuilderForXml(interchange, notifier);

				case ApplicationCodeList.Codes.UniversalDataMessaging:
				case ApplicationCodeList.Codes.NativeDataMessaging:
					return new EHubMessageBuilderForUniversalAndNative(interchange, notifier);

				case ApplicationCodeList.Codes.CIM:
				case ApplicationCodeList.Codes.SGCustomsCMD:
					return new EHubMessageBuilderForCIM(interchange, notifier);

				case ApplicationCodeList.Codes.Inttra:
				case ApplicationCodeList.Codes.ShippingLineEHubMessaging:
				case ApplicationCodeList.Codes.ZACustoms:
					return new EHubMessageBuilderForEdifact(interchange, notifier);

				case ApplicationCodeList.Codes.NZMAFeBACCa:
				case ApplicationCodeList.Codes.NZCustoms:
					return new EHubMessageDirectorForNZCustoms(interchange, notifier).CreateBuilder();

				case ApplicationCodeList.Codes.AUCMR:
					return new EHubMessageBuilderForAUCustoms(interchange, notifier);

				case ApplicationCodeList.Codes.CACustoms:
				case ApplicationCodeList.Codes.CAACI:
				case ApplicationCodeList.Codes.CAIMP:
				case ApplicationCodeList.Codes.CAEXP:
					return new EHubMessageBuilderForCanadianCustoms(interchange, notifier);

				case ApplicationCodeList.Codes.GbCustomsDeclarationServices:
					return new EHubMessageBuilderForGBCustoms(interchange, notifier);

				case ApplicationCodeList.Codes.GlobalElectronicInvoice:
					return new EHubMessageBuilderForGEI(interchange, notifier);

				case ApplicationCodeList.Codes.GlobalElectronicPayment:
					return new EHubMessageBuilderForGEP(interchange, notifier);

				case ApplicationCodeList.Codes.USeManifest:
				case ApplicationCodeList.Codes.USAMA:
				case ApplicationCodeList.Codes.USAMS:
				case ApplicationCodeList.Codes.USCustomsImport:
				case ApplicationCodeList.Codes.USCustomsExport:
				case ApplicationCodeList.Codes.USExportManifest:
				case ApplicationCodeList.Codes.StowPlan:
					return new EHubMessageBuilderForUSCustoms(interchange, notifier);

				case ApplicationCodeList.Codes.USCustomsDIS:
					return new EHubMessageDirectorForUSDIS(interchange, notifier).CreateBuilder();

				case ApplicationCodeList.Codes.eHub:
					return new EHubMessageBuilderForEHub(interchange, notifier);

				case ApplicationCodeList.Codes.Telematics:
					return new EHubMessageBuilderForTelematics(interchange, notifier);
				case ApplicationCodeList.Codes.HKTraxon:
					return new EHubMessageBuilderForHKCustoms(interchange, notifier);

				case ApplicationCodeList.Codes.GenericMessageDelivery:
					return new EHubMessageBuilderForGenericMessageDelivery(interchange, notifier);

				case ApplicationCodeList.Codes.AUCustomsNEXDOC:
					return new EHubMessageBuilderForAUCustomsNEXDOC(interchange, notifier);

				case ApplicationCodeList.Codes.ITCustoms:
					return new EHubMessageBuilderForITCustoms(interchange, notifier);

				case ApplicationCodeList.Codes.TWCustoms:
					return new EHubMessageDirectorForTWCustoms(interchange, notifier).CreateBuilder();

				case ApplicationCodeList.Codes.UYCustoms:
					return new EHubMessageBuilderForUYCustoms(interchange, notifier);

				case ApplicationCodeList.Codes.TRCustoms:
					return new EHubMessageBuilderForTRCustoms(interchange, notifier);

				case ApplicationCodeList.Codes.ESCustomsMessage:
					return new EHubMessageBuilderForESCustoms(interchange, notifier);

				default:
					return null;
			}
		}
	}
}
