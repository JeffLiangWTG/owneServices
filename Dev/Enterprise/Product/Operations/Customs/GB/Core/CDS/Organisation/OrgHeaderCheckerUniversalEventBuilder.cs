using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.CDS.Organisation
{
	public class OrgHeaderCheckerUniversalEventBuilder
	{
		OrgHeaderCheckerUniversalEventBuilder()
		{
		}

		public OrgHeaderCheckerUniversalEventBuilder(OrgHeader orgHeader, OrgCusCode orgCusCode)
		{
			this.orgHeader = Argument.NotNull(orgHeader, nameof(orgHeader));
			this.orgCusCode = Argument.NotNull(orgCusCode, nameof(orgCusCode));
		}

		public UniversalEvent BuildUniversalEvent()
		{
			switch (orgCusCode.OK_CodeType)
			{
				case OrgCusCode.CodeTypes.VATCode:
					{
						var universalEvent = BuildBaseUniversalEvent();
						AddVATContextCollection(universalEvent);
						return universalEvent;
					}
				case OrgCusCode.EuropeanUnionSharedCodeTypes.Eori:
					{
						var universalEvent = BuildBaseUniversalEvent();
						AddEORIContextCollection(universalEvent);
						return universalEvent;
					}
				default:
					return null;
			}
		}

		public static string ConvertToXml(UniversalEvent universalEvent)
		{
			using var stream = (SubStreamableStream)new MemoryStream();
			using var reader = new StreamReader(stream);
			new XmlWriter().WriteXML(universalEvent, stream, false, UniversalXmlInfo.Namespace_2011_11);
			stream.Flush();
			stream.Position = 0;
			var xml = reader.ReadToEnd();
			return xml;
		}

		UniversalEvent BuildBaseUniversalEvent()
		{
			return new UniversalEvent
			{
				EventTime = ZDateTimeOffset.Now,
				EventType = Events.ServiceRequestedCode,
				EventReference = CreateEventReference(),
				DataContext = CreateDataContext(),
				ContextCollection = new List<Context>
				{
					new Context
					{
						Type = "Key",
						Value = GetCredentialKey(),
					},
				},
			};
		}

		void AddVATContextCollection(UniversalEvent universalEvent)
		{
			universalEvent.ContextCollection.InsertRange(0, new List<Context>
			{
				new Context
				{
					Type = "NotificationType",
					Value = "VAT",
				},
				new Context
				{
					Type = "EntryNumber",
					Value = orgCusCode.OK_CustomsRegNo,
				},
			});
		}

		void AddEORIContextCollection(UniversalEvent universalEvent)
		{
			var notificationType = orgCusCode.OK_CustomsRegNo.StartsWith("XI") ? "NOP" : "EORI";
			var payload = JsonSerializer.SerializeToUtf8Bytes(new { eoris = new[] { (string)orgHeader.GetEuIdentificationNumber(Core.Constants.CountryCodes.UnitedKingdom) }, });

			universalEvent.ContextCollection.InsertRange(0, new List<Context>
			{
				new Context
				{
					Type = "NotificationType",
					Value = notificationType,
				},
				new Context
				{
					Type = "Payload",
					Value = Encoding.UTF8.GetString(payload),
				},
			});
		}

		IDataContextDataObject CreateDataContext()
		{
			var dataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2011_11);
			dataContext.AddDataSource(DataContextType.Organization, orgHeader.OH_Code);
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			return dataContext;
		}

		ZString GetCredentialKey()
		{
			return GBExtensions.GetCredentialsKeyForCurrentCompany();
		}

		static ZString CreateEventReference()
		{
			var parameters = new Dictionary<string, string>
			{
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "QUERY" },
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Service, "GBCustomsCDS" }
			};

			return StmALog.GenerateEventReference(ZString.Empty, parameters);
		}

		readonly OrgHeader orgHeader;
		readonly OrgCusCode orgCusCode;
	}
}
