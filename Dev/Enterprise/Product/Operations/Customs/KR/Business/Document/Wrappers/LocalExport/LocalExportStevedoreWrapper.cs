using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportStevedoreWrapper : NonPersistentBusinessObject
	{
		public LocalExportStevedoreWrapper(ILocalExportStevedore stevedore, BusinessObjectFactory factory)
		{
			Stevedore = stevedore;
		}
		public ILocalExportStevedore Stevedore { get; }

		public ZString AddressDetails
		{
			get
			{
				if (addressDetails.IsEmpty)
				{
					var strBuilder = new ZStringBuilder();
					if (!Stevedore.AddressLine1.IsEmpty)
					{
						strBuilder.Append(Stevedore.AddressLine1);
						if (!Stevedore.AddressLine2.IsEmpty)
						{
							strBuilder.Append(Stevedore.AddressLine2);
						}
					}
					addressDetails = strBuilder.ToStringWithDelimiterBetweenAppends(" ");
				}
				return addressDetails;
			}
		}
		ZString addressDetails;
	}
}
