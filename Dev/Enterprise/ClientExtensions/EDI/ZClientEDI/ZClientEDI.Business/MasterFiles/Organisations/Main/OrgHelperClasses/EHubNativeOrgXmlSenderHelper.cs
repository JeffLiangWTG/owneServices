using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.DataTransfer.Native;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public static class EHubNativeOrgXmlSenderHelper
	{
		public static void TakeSnapshotIfNeeded(this EDIOrgHeader orgHeader)
		{
			if (ValidForTakeSnapshot(orgHeader))
			{
				orgHeader.Snapshot = GetSnapShot(orgHeader);
			}
			else
			{
				orgHeader.Snapshot = null;
			}
		}

		public static void SetSnapshotCompanyDataChangedIfNeeded(this EDIOrgHeader orgHeader)
		{
			if (orgHeader.Snapshot != null)
			{
				orgHeader.Snapshot.CompanyDataChanged = orgHeader.CompanyDataCollection.OfType<OrgCompanyData>().Any(u => u.OB_IsDebtor && u.HasChanges);
			}
		}

		public static void SendMessageToEHubAndUpdateSnapshotIfNeeded(this EDIOrgHeader orgHeader)
		{
			if (ValidForTakeSnapshot(orgHeader))
			{
				var currentSnapshot = GetSnapShot(orgHeader);

				if (orgHeader.Snapshot == null || !currentSnapshot.AreEqual(orgHeader.Snapshot))
				{
					SendAndSaveMessageToEHubWithErrorReport(orgHeader);
				}

				orgHeader.Snapshot = currentSnapshot;
			}
			else
			{
				orgHeader.Snapshot = null;
			}
		}

		const string EHubReceiverId = "XHUB_US_CERTCAPTURE";

		internal static IDeliveryResult SendMessageToEHub(EDIOrgHeader orgHeader)
		{
			var nativeXmlSerializer = ObjectFactory.Get<IBusinessObjectWithDataContextInfoSerializer>("NativeXmlSerializer");
			var factory = orgHeader.Factory;
			var stream = factory.SubscribeForDispose(nativeXmlSerializer.SerializeToStream(orgHeader, null, null));
			var context = new DeliveryContext(orgHeader.Factory)
			{
				ParentInfo = EntityInfo.New(orgHeader),
				ApplicationCode = ApplicationCodeList.Codes.NativeDataMessaging,
				MessageTypeCode = EDIMessageTypeList.Codes.XDC,
				MessageSubTypeCode = orgHeader.GetType().GetNativeMessageSubTypeFromObjectType(),
				Notifications = new NotificationBuffer()
			};

			var delivery = new EDIMessageDelivery();
			return delivery.Deliver(context, new[] { CommunicationsMode }, new DeliveryStreamWrapperUXML(stream, context.ParentInfo)).Single();
		}

		static bool ValidForTakeSnapshot(EDIOrgHeader orgHeader)
		{
			return EDIDataRegistry.Instance.SendOrganizationDataToCertCapture.Value.EnableSend
					&& !orgHeader.IsDeleted
					&& orgHeader.IsInDatabase
					&& orgHeader.OH_IsActive
					&& orgHeader.CompanyDataCollection.OfType<OrgCompanyData>().Any(u => u.OB_IsDebtor)
					&& GetMainUSAddress(orgHeader) != null;
		}

		static SnapshotForEHubNativeOrg GetSnapShot(EDIOrgHeader orgHeader)
		{
			var mainAddress = GetMainUSAddress(orgHeader);
			var mainARContact = orgHeader.Contacts.OfType<OrgContact>()
				.Where(u => u.OC_IsActive == true && u.Documents.OfType<OrgDocument>().Any(v => (v.OD_DocumentGroup == ContactType.Receivables.Code || v.OD_DocumentGroup == ContactType.All.Code) && v.OD_DefaultContact))
				.OrderByDescending(u =>
				{
					var orderIndex = 0;
					var documents = u.Documents.OfType<OrgDocument>().Where(v => (v.OD_DocumentGroup == ContactType.Receivables.Code || v.OD_DocumentGroup == ContactType.All.Code) && v.OD_DefaultContact).Select(w => w.OD_DocumentGroup).ToArray();
					if (documents.Length > 0)
					{
						orderIndex = documents.Contains(ContactType.Receivables.Code) ? 2 : 1;
					}

					return orderIndex;
				}).FirstOrDefault();

			return new SnapshotForEHubNativeOrg
			{
				CustomerNumber = orgHeader.OH_Code,
				CustomerName = orgHeader.OH_FullName,
				FEIN = GetFEIN(orgHeader),
				Phone = mainAddress?.OA_Phone,
				Fax = mainAddress?.OA_Fax,
				Address1 = mainAddress?.OA_Address1,
				Address2 = mainAddress?.OA_Address2,
				City = mainAddress?.OA_City,
				State = mainAddress?.OA_State,
				Country = mainAddress?.OA_RN_NKCountryCode,
				Zip = mainAddress?.OA_PostCode,
				ContactName = mainARContact?.OC_ContactName,
				EmailAddress = mainARContact?.OC_Email,
			};
		}

		static ZString GetFEIN(EDIOrgHeader orgHeader)
		{
			return orgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, Core.Constants.CountryCodes.UnitedStates);
		}

		static OrgAddress GetMainUSAddress(EDIOrgHeader orgHeader)
		{
			var mainAddress = orgHeader.Addresses.OfType<OrgAddress>()
				.FirstOrDefault(u => u.OA_IsActive
									&& u.OA_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates
									&& u.CapabilitiesCollection.Any(v => v.PZ_IsMainAddress && v.PZ_AddressType == OrgConstants.AddressType.Office));

			return mainAddress;
		}

		static void SendAndSaveMessageToEHubWithErrorReport(EDIOrgHeader orgHeader)
		{
			using (orgHeader.Factory.AddDisposableService())
			{
				var result = SendMessageToEHub(orgHeader);

				if (result.Succeeded)
				{
					orgHeader.Factory.Save();
				}
				else
				{
					ErrorReporter.ReportOnce("EHubNativeOrgXmlSender_SendMessageToEHub", result.FailureReason, result.Exception);
				}
			}
		}

		[ThreadStatic]
		static EDICommunicationsMode communicationsMode;

		static EDICommunicationsMode CommunicationsMode
		{
			get
			{
				if (communicationsMode == null)
				{
					var factory = new ReadOnlyBusinessObjectFactory();
					communicationsMode = factory.New<EDICommunicationsMode>();
					communicationsMode.EK_Module = new OrgHeaderWorkflowDescriptor().Code;
					communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
					communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
					communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
					communicationsMode.EK_Destination = EHubReceiverId;
				}

				return communicationsMode;
			}
		}
	}
}
