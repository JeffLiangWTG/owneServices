using System;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolWithSingleTrue : CodeDescriptionBool
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member CodeAndDescriptionReadOnly")]
		[ReadOnlyMember("CodeAndDescriptionReadOnly")]
		public override ZString EnglishDescription
		{
			get => base.EnglishDescription;
			set
			{
				if (base.EnglishDescription != value)
				{
					base.EnglishDescription = value;
					ValidateURL();
				}
			}
		}

		protected override void ValidateValueCore()
		{
			base.ValidateValueCore();

			if (ParentCollection != null && ParentCollection.Cast<CodeDescriptionBoolWithSingleTrue>().Count(x => x.Bool) != 1)
			{
				BoolInfo.AddError(errorMessage);
			}
		}

		void ValidateURL()
		{
			EnglishDescriptionInfo.ClearAllNotifications();

			if (EnglishDescription.IsEmpty)
			{
				EnglishDescriptionInfo.AddError((NoResString)"URL should not be empty.");
			}
			else
			{
				if (!(Uri.TryCreate(EnglishDescription, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)))
				{
					EnglishDescriptionInfo.AddError((NoResString)"URL format is incorrect.");
				}
			}
		}

		string errorMessage => (NoResString)"One URL must be specified as the URL that the service will use.";

		protected override void RunPreSaveValidationCore()
		{
			ValidateURL();
			base.RunPreSaveValidationCore();
		}

		CodeDescriptionBoolWithSingleTrueCollection ParentCollection => (CodeDescriptionBoolWithSingleTrueCollection)GetParentCollection(this, typeof(CodeDescriptionBoolWithSingleTrueCollection));

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CodeDescriptionBoolWithSingleTrue();
		}
	}
}
