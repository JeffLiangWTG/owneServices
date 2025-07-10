//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCollectionBatchValidation
//
//    This class should be used for overriding validation in AutoAccCollectionBatchValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.Riba
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;

	public class AccCollectionBatchValidation : AutoAccCollectionBatchValidation
	{
		public AccCollectionBatchValidation(AutoAccCollectionBatch parent) : base(parent)
		{ }

		protected new AccCollectionBatch Parent
		{
			get { return base.Parent as AccCollectionBatch; }
		}

		protected override void CheckACB_CollectionFileFormat()
		{
			base.CheckACB_CollectionFileFormat();
			MandatoryValidation.CheckEntered(Parent.ACB_CollectionFileFormatInfo);
			if (!Parent.ACB_CollectionFileFormatInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.ACB_CollectionFileFormatInfo);
			}
			if (!Parent.ACB_CollectionFileFormatInfo.HasErrors())
			{
				if (Parent.ACB_RX_NKCurrency != Core.Constants.CurrencyCodes.EuropeanUnion && Parent.ACB_CollectionFileFormat == CollectionFileFormatList.Codes.sepaFormat)
				{
					Parent.ACB_CollectionFileFormatInfo.AddError(Res.GetString("3436b57c-a590-4856-8132-0183a731d0a2", "SEPA format can only be chosen when currency is EUR."));
				}
			}
			if (Parent.ACB_CollectionFileFormat == CollectionFileFormatList.Codes.unknown)
			{
				Parent.ACB_CollectionFileFormatInfo.AddWarning(Res.GetString("9ccc83fd-0dd5-4e71-9cc7-48e21735133b", "While collection file format is unknown, EDI message will not be processed properly."));
			}
		}

		protected override void CheckACB_Type()
		{
			base.CheckACB_Type();
			MandatoryValidation.CheckEntered(Parent.ACB_TypeInfo);
			if (!Parent.ACB_TypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.ACB_TypeInfo);
			}
		}

		protected override void CheckACB_AB()
		{
			base.CheckACB_AB();
			if (!Parent.ACB_ABInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.ACB_ABInfo);
				if (Parent.BankAccount != null && Parent.BankAccount.AB_RX_NKAccountCurrency != Parent.ACB_RX_NKCurrency)
				{
					Parent.ACB_ABInfo.AddError(Res.GetString("ea945e03-27de-4f1e-b51a-6f0fff2e3525", "Bank account must have same currency as the batch."));
				}
			}
		}

		protected override void CheckACB_TotalAmount()
		{
			base.CheckACB_TotalAmount();
			if (!Parent.ACB_TotalAmountInfo.HasErrors())
			{
				if (Parent.ACB_TotalAmount == 0 && !Parent.IsCancelled)
				{
					Parent.ACB_TotalAmountInfo.AddError(Res.GetString("2a36b45a-f946-409f-8650-aeac19bc042f", "Batch total amount can not be 0. If you would like to cancel this batch, please run 'Cancel Batch' in the main menu."));
				}
				else if (Parent.ACB_TotalAmount < 0)
				{
					Parent.ACB_TotalAmountInfo.AddError(Res.GetString("382761e9-0104-41e6-a726-4858ca3130b5", "Batch total amount can not be negative."));
				}
			}
		}
	}
}

