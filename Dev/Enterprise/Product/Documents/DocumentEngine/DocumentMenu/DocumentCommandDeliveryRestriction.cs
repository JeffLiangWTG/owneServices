using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine
{
	public class DocumentCommandDeliveryRestriction : StmMenuDeliveryRestriction
	{
		public DocumentCommandDeliveryRestriction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		DocumentCommand command;
		internal DocumentCommand Command => command ?? (command = Factory.Load<DocumentCommand>(SDR_SU));

		public bool SDR_RN_NKOriginCountryCode_ReadOnly => !(Command?.Parent is IOriginDestinationForDocumentDeliveryRestriction);
		public bool SDR_RN_NKDestinationCountryCode_ReadOnly => !(Command?.Parent is IOriginDestinationForDocumentDeliveryRestriction);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SDR_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
		}

		protected bool SDR_DeliveryRestrictionMacro_ReadOnly => SDR_DeliveryRestrictionType != nameof(DeliveryRestrictionType.UDF);

		[List("DeliveryRestrictionTypeList")]
		public override ZString SDR_DeliveryRestrictionType
		{
			get => base.SDR_DeliveryRestrictionType;
			set
			{
				base.SDR_DeliveryRestrictionType = value;
				if (value != nameof(DeliveryRestrictionType.UDF))
				{
					if (!SDR_DeliveryRestrictionMacro.IsEmpty)
					{
						SDR_DeliveryRestrictionMacro = string.Empty;
					}
				}
			}
		}

		public override ZString SDR_RN_NKOriginCountryCode
		{
			get => base.SDR_RN_NKOriginCountryCode;
			set
			{
				base.SDR_RN_NKOriginCountryCode = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateSDR_DeliveryRestrictionType();
				}
			}
		}

		public override ZString SDR_RN_NKDestinationCountryCode
		{
			get => base.SDR_RN_NKDestinationCountryCode;
			set
			{
				base.SDR_RN_NKDestinationCountryCode = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateSDR_DeliveryRestrictionType();
				}
			}
		}

		public ZString DeliveryRestrictionMacroViewValue => SDR_DeliveryRestrictionMacro;

		public CodeDescriptionPairList DeliveryRestrictionTypeList => deliveryRestrictionTypeList ?? (deliveryRestrictionTypeList = DeliveryRestrictionTypeHelper.UserDefinedDeliveryRestrictionTypeList());
		CodeDescriptionPairList deliveryRestrictionTypeList;

		protected override StmMenuDeliveryRestrictionValidation GetNewValidation()
		{
			return new DocumentCommandDeliveryRestrictionValidation(this);
		}

		#region For Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			SDR_DeliveryRestrictionMacro = "<TestMacro>";
		}

#endif
		#endregion
	}
}
