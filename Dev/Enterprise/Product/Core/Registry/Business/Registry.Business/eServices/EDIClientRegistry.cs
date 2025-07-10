using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.eServices
{
	public sealed class EDIClientRegistry : RegistryItemSet
	{
		#region Singleton Instance

		[ThreadStatic]
		static EDIClientRegistry instance;

		public static EDIClientRegistry Instance
		{
			get
			{
				return instance ?? (instance = new EDIClientRegistry());
			}
		}

		#endregion

		public override bool IsForProductivityWise => true;

		#region Categories

		public abstract class Categories : eServicesRegistry.Categories
		{
			public static MultilingualString eServices_EDIClient_CertificateExpiry { get { return CombineCategories(eServices_EDIClient, ResString.GetMultilingualString("fc0ae341-11b4-4664-afcf-72c93eeaadb5", "Certificate Expiry")); } }
		}

		#endregion

		#region EDIClient Certificate Expiry
		public GuidRegistryItem ClientCertificateExpiryNotificationGroup
		{
			get
			{
				var multilingualRegistryItem = (IMultilingualRegistryItem)RawDataRegistry.Instance.NotificationGroup;
				var multilingualString = ResString.GetMultilingualString("5489340e-a7a4-463e-b89a-b6577a7c1fd7", "[System] > [Registry] > [{0}] > [{1}]", multilingualRegistryItem.CategoryMultilingual.Replace("/", "] > ["), multilingualRegistryItem.CaptionMultilingual);
				var certificateExpiryEmailRecipientsDescription = ResString.GetMultilingualString("fc71809c-bd7e-4874-a2bc-2e0af0a9629a", @"The staff group that will be notified when the EDI Client Certificate is about to expire or has been expired. The fallback level rule applied when: 
	- If the Certificate Expiry Notification Group doesn't contain a valid Email address for this {0} System, it will look for System Notification Group ({1}).
	- If the System Notification Group doesn't contain a valid Email address, the notification will send to all the staff listed in the {0} System.", Core.Constants.ProductName, multilingualString);

				return GetItem("ClientCertificateExpiryNotificationGroup",
					() => new GuidRegistryItem("ClientCertificateExpiryNotificationGroup",
						Categories.eServices_EDIClient_CertificateExpiry,
						ResString.GetMultilingualString("e81789c0-19d1-4fad-89fe-a1f24eaad042", "Email Recipients"),
						certificateExpiryEmailRecipientsDescription,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Core.Constants.Groups.PostMastersGroupPK)
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup)
					});
			}
		}

		public IntRegistryItem ClientCertificateExpiryNotificationDuration
		{
			get
			{
				return GetItem("ClientCertificateExpiryNotificationDuration", delegate
				{
					var result = new IntRegistryItem(
						"ClientCertificateExpiryNotificationDuration",
						Categories.eServices_EDIClient_CertificateExpiry,
						ResString.GetMultilingualString("286e3c21-b648-4e64-ac18-cb9600891111", "Notification Window (days before/after)"),
						ResString.GetMultilingualString("3b9bb93a-02f3-4025-a43b-8d3943598ef6", "Configure the notification window to define how many days before the EDI Client Certificate expiration the system should send notifications, as well as how many days after the expiration."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 30,
						minValue: 0,
						maxValue: int.MaxValue);
					return result;
				});
			}
		}
		#endregion
	}
}
