using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.AU.Declaration.Business.XmlSerializers")]

	public class ContingencyDataEmailAddress : RegistryBusinessObject
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ContingencyDataEmailAddress();
		}

		protected override int CodeMaxLengthDefaultValue
		{
			get { return 40; }
		}

		protected override int MaxDescriptionLength
		{
			get { return MAX_DESCRIPTION_LENGTH; }
		}

#if DEBUG
		internal int MaxDescriptionLengthInternal
		{
			get { return MaxDescriptionLength; }
		}
#endif

		internal const int MAX_DESCRIPTION_LENGTH = 120;

		protected override string CodeDisplayName
		{
			get { return "Description"; }
		}

		protected override bool IsCodeUniqueInCollection
		{
			get { return false; }
		}

		protected override void ValidateDescriptionCore()
		{
			base.ValidateDescriptionCore();

			MandatoryValidation.CheckEntered(DescriptionInfo, "Email Address");

			var emailAddresses = ((ZString)Description).Split(';');
			foreach (var email in emailAddresses)
			{
				if (!EmailAddressValidation.IsEmailAddressValid(email))
				{
					DescriptionInfo.AddError(InvalidEmail);
					break;
				}
			}
		}
		internal const string InvalidEmail = "One of the email addresses is not valid.";
	}
}
