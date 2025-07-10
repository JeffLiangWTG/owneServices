using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Integration
{
	[Flags]
	public enum ChargePosterBehaviours
	{
		None = 0,

		AutoRateDSB = 1,
		APPostDSB = 2,
		ARPostDSB = 4,

		ARAPPostNonDSB = 8,
		SendEmail = 16,

		PostNegativeCost = 32
	}

	public interface IAccIntegrationDataProvider : IControllerIDProvider
	{
		BusinessObjectFactory Factory { get; }
		ChargePosterBehaviours Action { get; }
		Action OnIntegrated { get; }
		ZGuid[] DisbursementChargeCodes { get; }
		ZString ReferenceID { get; }
		ZString JobType { get; }
		IAccInvoiceDataProvider[] InvDataProviders { get; }
		bool SupportIntegration { get; }
		void LogPostingResult(string message);

		ZGuid AutoPostingEmailRecipient { get; }
		GlbCompany Company { get; }
	}

	public interface ICustomsDisbursementChargePoster
	{
		IAutoBillingResult RaiseInvoices(IAccIntegrationDataProvider provider);
	}

	public interface IAutoBillingResult
	{
		ZBool WasSuccessful { get; }
		ZString Message { get; }

		ZBool HasChanges { get; }
	}

	public struct AutoBillingResult : IAutoBillingResult
	{
		public ZBool WasSuccessful { get; set; }
		public ZString Message { get; set; }
		public ZBool HasChanges { get; set; }

		public void SetResult(IAutoBillingResult result)
		{
			WasSuccessful = result.WasSuccessful;
			Message = result.Message;
			HasChanges = result.HasChanges;
		}
	}

	[Serializable]
	public class CustomsInvoiceRaiseException : ZException
	{
		public CustomsInvoiceRaiseException(ZString message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected CustomsInvoiceRaiseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	public class CustomsDisbursementChargePosterCreator
	{
		public ICustomsDisbursementChargePoster GetNewChargePoster(ChargePosterBehaviours chargePosterBehaviours, IEnumerable<ZGuid> customsDSBChargeCodes)
		{
			return (ICustomsDisbursementChargePoster)Activator.CreateInstance(ObjectFactory.GetType<ICustomsDisbursementChargePoster>(), new object[] { chargePosterBehaviours, customsDSBChargeCodes });
		}
	}
}
