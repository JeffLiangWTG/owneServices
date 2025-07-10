using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HouseBillOfLadingTermsAndConditions : RegistryImage
	{
		#region Schema

		protected new abstract class Schema : RegistryImage.Schema
		{
			public const string DeliveryMode = "DeliveryMode";
		}

		#endregion

		#region Validation Overrides

		protected override bool IsEmptyImageAllowed
		{
			get { return !IsDeliveryModeALL; }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDeliveryMode();
		}

		protected override bool IsCodeUniqueInCollection
		{
			get { return false; }
		}

		#endregion

		#region Delivery Mode

		[ListAttribute("DeliveryModeList", AllowOnlyTheseValues = true)]
		[MaxLength(3)]
		public ZString DeliveryMode
		{
			get { return deliveryMode; }
			set
			{
				CheckMaximumLength(DeliveryModeInfo, value);
				SetNonPersistentPropertyValue<ZString>(DeliveryModeInfo, ref deliveryMode, value);
				if (!IsValidationSuspended)
				{
					ValidateDeliveryMode();
				}
			}
		}

		public ZPropertyInfo DeliveryModeInfo
		{
			get { return GetZPropertyInfo(nameof(DeliveryMode)); }
		}

		public void ValidateDeliveryMode()
		{
			DeliveryModeInfo.ClearAllNotifications();

			if (!AllFieldsEmpty && ParentCollections.Count == 1 && (ParentCollections.First() is HouseBillOfLadingTermsAndConditionsCollection))
			{
				bool foundALL = IsDeliveryModeALL;
				bool foundDuplicate = false;

				foreach (HouseBillOfLadingTermsAndConditions element in ParentCollections.Last())
				{
					if (element != this && element.Code == Code)
					{
						if (element.IsDeliveryModeALL)
						{
							foundALL = true;
						}

						if (element.DeliveryMode == DeliveryMode)
						{
							foundDuplicate = true;
							break;
						}
					}
				}

				if (!foundALL)
				{
					DeliveryModeInfo.AddError(Res.GetString("c8213961-ca65-4375-a346-77339d3e4b7b", "At least one row for this Code should have Delivery Mode set to \"ALL\"."));
				}
				else if (foundDuplicate)
				{
					DeliveryModeInfo.AddError(Res.GetString("ff100c20-03fd-4eab-be04-39aa458745fa", "There is already another row for this Code with this Delivery Mode."));
				}
			}

			MandatoryValidation.CheckEntered(DeliveryModeInfo);
			ListValidation.ErrorIfInvalidCode(DeliveryModeInfo);
		}

		bool AllFieldsEmpty
		{
			get { return (Code.IsEmpty && Image == null && DeliveryMode.IsEmpty); }
		}

		ZString deliveryMode;

		#endregion

		#region Delivery Mode List

		internal const string FWB = "FWB";
		internal const string SWB = "SWB";

		public CodeDescriptionPairList DeliveryModeList
		{
			get
			{
				deliveryModeList = deliveryModeList ?? new CodeDescriptionPairList(OLookUpEditType.PrintCopyType);

				if (Code == FWB || Code == SWB)
				{
					return new CodeDescriptionPairList { deliveryModeList[deliveryModeList.IndexOfCode(nameof(PrintCopyType.ALL))] };
				}

				return deliveryModeList;
			}
		}

		CodeDescriptionPairList deliveryModeList;

		#endregion

		#region Write/Read XML

		protected override void WriteMoreElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.DeliveryMode, DeliveryMode);
			base.WriteMoreElements(writer);
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			DeliveryMode = reader.ReadElementString(Schema.DeliveryMode);
			base.ReadMoreElements(reader);
		}

		#endregion

		public bool IsDeliveryModeALL
		{
			get { return (DeliveryMode == nameof(PrintCopyType.ALL)); }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HouseBillOfLadingTermsAndConditions();
		}

		#region Implementation

		public override bool Equals(object obj)
		{
			var other = obj as HouseBillOfLadingTermsAndConditions;
			return other != null && other.DeliveryMode == DeliveryMode && base.Equals(other);
		}

		public override int GetHashCode()
		{
			var hashCode = base.GetHashCode();
			return HashCodeHelper.GetCompositeHashCode(hashCode, DeliveryMode.GetHashCode());
		}

		#endregion
	}
}
