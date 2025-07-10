using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoiceLineOverrideForEditingDescription : InvoiceLineOverride
	{
		#region Schema

		public abstract class Schema
		{
			public const string OriginalDescription = "OriginalDescription";
		}

		#endregion

		public InvoiceLineOverrideForEditingDescription(DependentTransactionLine line)
			: base(line)
		{
		}

		#region OriginalDescription

		[MaxLength(1024)]
		public ZString OriginalDescription
		{
			get
			{
				return (ZString)Line.AL_DescInfo.OriginalValue;
			}
		}

		public ZPropertyInfo OriginalDescriptionInfo
		{
			get { return this.GetZPropertyInfo(Schema.OriginalDescription); }
		}

		#endregion

		#region AL_Desc

		[MaxLength(1024)]
		public ZString AL_Desc
		{
			get
			{
				return Line.AL_Desc;
			}
			set
			{
				if (value != Line.AL_Desc)
				{
					Line.AL_Desc = value;

					if (Line.AL_LineType == TransactionLineTypes.Revenue)
					{
						var chargeForLine = Factory.LoadTop1<BaseCharge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, Line.PK));
						if (chargeForLine != null)
						{
							chargeForLine.JR_Desc = value;
						}
					}

					if (!IsValidationSuspended)
					{
						ValidateAL_Desc();
					}
				}
				AL_DescInfo.RefreshBinding();
			}
		}

		void ValidateAL_Desc()
		{
			AL_DescInfo.ClearAllNotifications();

			if (Line.AL_DescInfo.HasChanges)
			{
				if (Line.TransactionHeader.AH_GC.ToGuid() != Env.CurrentCompany.PK)
				{
					AL_DescInfo.AddError(Res.GetString("d8165799-2c57-4b40-ad9f-d454d61463ef", @"Overriding the Transaction Description is not allowed on lines belonging to a sister company invoice."));
				}

				if (!AL_DescInfo.HasErrors())
				{
					if (!ChargeCodeAllowsDescriptionOverride())
					{
						if (Line.ChargeCode != null)
						{
							AL_DescInfo.AddError(Res.GetString("692c4c68-b8e8-422c-b0e6-882d50b72623", @"The charge code '{0}' is not in the list of allowed charge codes for overriding the line description. You can configure the list by changing the registry setting '{1}'",
								Line.ChargeCode.AC_Code,
								AccountingConfigurationRegistry.Instance.AllowChargeDescriptionOverrideOnPostedARInvoice.Caption));
						}
						else
						{
							AL_DescInfo.AddError(Res.GetString("ef65e69f-ae83-47fe-a889-c82ec198377d", @"Overriding the Transaction Description is only allowed on lines with a charge code"));
						}
					}
				}
			}
		}

		bool ChargeCodeAllowsDescriptionOverride()
		{
			bool chargeCodeIsInRegistry = false;

			if (Line.ChargeCode != null)
			{
				var chargeCodePks = AccountingConfigurationRegistry.Instance.AllowChargeDescriptionOverrideOnPostedARInvoice.GetAsGuidArray();
				var chargeCodes = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, chargeCodePks));

				foreach (var chargeCode in chargeCodes)
				{
					if (Line.ChargeCode.AC_Code == chargeCode.AC_Code)
					{
						chargeCodeIsInRegistry = true;
						break;
					}
				}
			}
			return chargeCodeIsInRegistry;
		}

		public ZPropertyInfo AL_DescInfo
		{
			get { return this.GetZPropertyInfo(nameof(AL_Desc)); }
		}

		protected override void RunPreSaveValidationCore()
		{
			ValidateAL_Desc();
		}

		#endregion
	}
}
