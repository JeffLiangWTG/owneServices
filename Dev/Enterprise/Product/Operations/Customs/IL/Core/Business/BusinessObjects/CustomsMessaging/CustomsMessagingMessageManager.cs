using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class CustomsMessagingMessageManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CustomsMessagingMessageManager(BusinessObject owner)
			: base(owner.Factory)
		{
			this.owner = owner;
		}
		
		public static CustomsMessagingMessageManager New(BusinessObject owner)
		{
			Argument.NotNull(owner, "owner");

			CustomsMessagingMessageManager result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(owner);
			}
			else
			{
				result = new CustomsMessagingMessageManager(owner);
			}

			return result;
		}

		#region Properties

		public CustomsMessagingEDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new CustomsMessagingEDIMessageCollection(owner);
				}

				return fMessages;
			}
		}
		CustomsMessagingEDIMessageCollection fMessages;

		#endregion

		protected readonly BusinessObject owner;
		protected delegate CustomsMessagingMessageManager NewDelegate(BusinessObject owner);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
	}
}
