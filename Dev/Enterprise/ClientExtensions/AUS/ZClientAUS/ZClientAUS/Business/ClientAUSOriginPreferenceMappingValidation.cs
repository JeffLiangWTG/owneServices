//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientAUSOriginPreferenceMappingValidation
//
//    This class should be used for overriding validation in AutoClientAUSOriginPreferenceMappingValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Business
{
	public class ClientAUSOriginPreferenceMappingValidation : AutoClientAUSOriginPreferenceMappingValidation
	{
		public ClientAUSOriginPreferenceMappingValidation(AutoClientAUSOriginPreferenceMapping parent) : base(parent)
		{
		}

		#region Factory

		public BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}

				return fFactory;
			}
		}

		BusinessObjectFactory fFactory;

		#endregion

		#region Validation

		protected override void CheckT7_OH_Importer()
		{
			base.CheckT7_OH_Importer();

			if (!Parent.T7_OH_Importer.IsValid)
			{
				Parent.T7_OH_ImporterInfo.AddError("The Importer code entered is not valid");
			}
		}

		protected override void CheckT7_OH_Supplier()
		{
			base.CheckT7_OH_Supplier();

			if (!Parent.T7_OH_Supplier.IsValid)
			{
				Parent.T7_OH_SupplierInfo.AddError("The Supplier code entered is not valid");
			}
		}

		protected override void CheckT7_RN_NKOrigin()
		{
			base.CheckT7_RN_NKOrigin();

			MandatoryValidation.CheckEntered(Parent.T7_RN_NKOriginInfo, "Origin");

			if (!Parent.T7_RN_NKOrigin.IsEmpty && Parent.HasChanges)
			{
				ZQuery keyFilter = new ZQuery(ClientAUSOriginPreferenceMappingSchema.T7_OH_Importer, Parent.T7_OH_Importer);
				keyFilter.AddToFilter(ClientAUSOriginPreferenceMappingSchema.T7_OH_Supplier, Parent.T7_OH_Supplier);
				keyFilter.AddToFilter(ClientAUSOriginPreferenceMappingSchema.T7_RN_NKOrigin, Parent.T7_RN_NKOrigin);
				keyFilter.AddToFilter(ClientAUSOriginPreferenceMappingSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				ClientAUSOriginPreferenceMapping mapping = (ClientAUSOriginPreferenceMapping)Factory.LoadTop1(typeof(ClientAUSOriginPreferenceMapping), keyFilter);
				if (mapping != null)
				{
					Parent.T7_RN_NKOriginInfo.AddError("Mapping already exists for this Importer, Supplier & Origin combination");
				}
			}
		}

		#endregion
	}
}
