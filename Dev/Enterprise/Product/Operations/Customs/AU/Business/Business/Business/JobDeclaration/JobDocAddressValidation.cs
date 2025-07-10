using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobDocAddressValidationForCMR : JobDocAddressValidation
	{
		public JobDocAddressValidationForCMR(JobDocAddress parent, JobDeclaration declaration)
			: base(parent)
		{
			this.declaration = declaration;
			isThisForImporterDeliveryAddress = declaration.IsImportCMR && (parent.E2_AddressType == DocAddressTypes.Codes.ImporterPickupDeliveryAddress || parent.E2_AddressType == DocAddressTypes.Codes.ConsigneePickupDeliveryAddress);
			isForDepotAddress = declaration.IsExport && parent.E2_AddressType == DocAddressTypes.Codes.CustomsDepotAddress;
			isForWarehouseAddress = parent.E2_AddressType == DocAddressTypes.Codes.CustomsWarehouseAddress;
		}

		readonly bool isThisForImporterDeliveryAddress;
		readonly bool isForDepotAddress;
		readonly bool isForWarehouseAddress;
		readonly JobDeclaration declaration;

		protected override void CheckE2_Address1()
		{
			if (Parent.E2_AddressOverride && isThisForImporterDeliveryAddress)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_Address1Info);
				CheckChangeAndWarnAmendment(Parent.E2_Address1Info);
			}
		}

		protected override void CheckE2_City()
		{
			if (Parent.E2_AddressOverride && isThisForImporterDeliveryAddress)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_CityInfo);
				CheckChangeAndWarnAmendment(Parent.E2_CityInfo);
			}
		}

		protected override void CheckE2_State()
		{
			if (Parent.E2_AddressOverride && isThisForImporterDeliveryAddress)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_StateInfo);
				CheckChangeAndWarnAmendment(Parent.E2_StateInfo);
			}
		}

		protected override void CheckE2_Postcode()
		{
			if (Parent.E2_AddressOverride && isThisForImporterDeliveryAddress)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_PostcodeInfo);
				CheckChangeAndWarnAmendment(Parent.E2_PostcodeInfo);
				if (declaration != null && !declaration.IsValidationSuspended)
				{
					ZString postCodeError;
					ZString postCodeWarning;
					declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
					if (!postCodeError.IsEmpty)
					{
						Parent.E2_PostcodeInfo.AddMessageError(postCodeError);
					}

					if (!postCodeWarning.IsEmpty)
					{
						Parent.E2_PostcodeInfo.AddWarning(postCodeWarning);
					}

					if (declaration.AddInfo.Validation.GetType() == typeof(CMRAddInfoDeclarationValidation))
					{
						((CMRAddInfoDeclarationValidation)declaration.AddInfo.Validation).ValidateZA_AQISInspectLocation_Hidden();
					}
				}
			}
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (isForDepotAddress)
			{
				if (Parent.Address != null)
				{
					if (declaration.WarehouseDocAddress.E2_OA_Address.IsEmpty)
					{
						Parent.E2_OA_AddressInfo.AddWarning("Depot address entered without a Bonded Warehouse.  Warehouse Establishment Code for Exports come from the Bonded Warehouse");
					}
				}
			}

			if (isForWarehouseAddress)
			{
				if (Parent.Address != null)
				{
					if (Parent.Address.LocalControlledPremisesID.IsEmpty)
					{
						if (declaration.IsImport)
						{
							Parent.E2_OA_AddressInfo.AddMessageError("The bonded warehouse doesn't have a warehouse code entered.");
						}
						else if (declaration.IsExport)
						{
							Parent.E2_OA_AddressInfo.AddMessageError("Customs Registration Number Required");
						}
					}
				}
			}

			if (!Parent.E2_AddressOverride && isThisForImporterDeliveryAddress)
			{
				if (Parent.Address == null)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_OA_AddressInfo);
				}
				else
				{
					OrgAddress address = Parent.Address;
					ZStringBuilder missingFields = new ZStringBuilder();
					if (address.OA_Address1.IsEmpty)
					{
						missingFields.Append("street,");
					}

					if (address.OA_City.IsEmpty)
					{
						missingFields.Append("city,");
					}

					if (address.OA_State.IsEmpty)
					{
						missingFields.Append("state,");
					}

					if (address.OA_PostCode.IsEmpty)
					{
						missingFields.Append("postcode");
					}

					if (!missingFields.IsEmpty)
					{
						ZStringBuilder message = new ZStringBuilder();
						message.Append("The following fields are missing: ");
						message.Append(missingFields.ToString().TrimEnd(','));
						message.Append(". Please press F3 in the organisation and enter more information in Organisation > Address.");
						Parent.E2_OA_AddressInfo.AddMessageError(message.ToString());
					}

					CheckChangeAndWarnAmendment(Parent.E2_OA_AddressInfo);

					if (declaration != null && !declaration.IsValidationSuspended)
					{
						ZString postCodeError;
						ZString postCodeWarning;
						declaration.DeliveryAddressPostCodeMessages(out postCodeError, out postCodeWarning);
						if (!postCodeError.IsEmpty)
						{
							Parent.E2_OA_AddressInfo.AddMessageError(postCodeError);
						}

						if (!postCodeWarning.IsEmpty)
						{
							Parent.E2_OA_AddressInfo.AddWarning(postCodeWarning);
						}

						if (declaration.AddInfo.Validation.GetType() == typeof(CMRAddInfoDeclarationValidation))
						{
							((CMRAddInfoDeclarationValidation)declaration.AddInfo.Validation).ValidateZA_AQISInspectLocation_Hidden();
						}
					}
				}
			}
		}

		public const string AmendmentWarning = "You have changed delivery address information which is mandatory reporting for import messages. An amendment message should be sent for this change. If you are not a licensed broker, then you can select cancecl and let a Broker make the change and lodge the amendment entry OR select 'save WITH entry changes' and provide the declaration to a Broker to lodge the amendment entry. A log is added against the user name if you select 'save without sending an amendment'.";
		void CheckChangeAndWarnAmendment(ZPropertyInfo addressFieldInfo)
		{
			if (declaration.CustomsEntryHeaders.HasEntryWithPostLodgeStatus &&
				addressFieldInfo.OriginalValue.CompareTo(addressFieldInfo.Value) != 0)
			{
				addressFieldInfo.AddWarning(AmendmentWarning);
			}
		}
	}
}
