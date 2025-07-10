using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.Wow
{
	public class WowStringToBusinessObjectFieldConverter : StringToBusinessObjectFieldConverter
	{
		protected WowStringToBusinessObjectFieldConverter(ZGuid mappingOrgPK) : base(mappingOrgPK)
		{
		}

		public static WowStringToBusinessObjectFieldConverter Instance
		{
			get
			{
				if (fInstance == null)
				{
					if (!GlbCompany.CurrentCompany.GC_OH_OrgProxy.IsValid)
					{
						throw new ArgumentException("CurrentCompany doesn't have an org proxy!");
					}
					fInstance = new WowStringToBusinessObjectFieldConverter(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				}
				return fInstance;
			}
		}
		[ThreadStatic]
		static WowStringToBusinessObjectFieldConverter fInstance;

		#region LoadPartForBuyerTakeOn

		public WoolworthsProduct LoadPartForBuyerTakeOn(
			BusinessObjectFactory factory, ZString referenceForError, ZString partno, INotifications notify, bool notifyErrorIfNoneFound)
		{
			ZQuery filter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.Equal, partno);
			WoolworthsProduct[] products = (WoolworthsProduct[])factory.Load(typeof(WoolworthsProduct), filter);

			WoolworthsProduct result = null;
			if (products.Length == 1)
			{
				result = products[0];
			}
			else if (products.Length > 1)
			{
				notify.Notify(new ErrorNotification(WowErrorType.MoreThan1PartNumber, referenceForError));
			}
			else if (products.Length == 0 && notifyErrorIfNoneFound)
			{
				notify.Notify(new WarningNotification(WowWarningType.NewPartNumberAdded, referenceForError));
			}
			return result;
		}

		#endregion

		#region Implementation

		protected override ZGuid UnmatchOrgPK
		{
			get { return WowDataRegistry.Instance.UnmatchedDataItemsAccount; }
		}

		protected override ZDateTime ParseDateTime(string valueAsString)
		{
			ZDateTime result;
			if (new ZString(valueAsString).IsEmpty)
			{
				result = ZDateTime.Empty;
			}
			else
			{
				string[] possibleFormats = new string[]
				{
					"dd-MM-yyyy",
					"d-MM-yyyy",
					"dd-M-yyyy",
					"d-M-yyyy",

					"dd/MM/yyyy",
					"d/MM/yyyy",
					"dd/M/yyyy",
					"d/M/yyyy",

					"dd-MM-yy",
					"d-MM-yy",
					"dd-M-yy",
					"d-M-yy",

					"dd/MM/yy",
					"d/MM/yy",
					"dd/M/yy",
					"d/M/yy",

					"yyyy/MM/dd",
					"yyyy/MM/d",
					"yyyy/M/dd",
					"yyyy/M/d",

					"yyyy-MM-dd",
					"yyyy-MM-d",
					"yyyy-M-dd",
					"yyyy-M-d",
				};
				DateTime date = DateTime.ParseExact(valueAsString, possibleFormats, System.Globalization.CultureInfo.CreateSpecificCulture("en-AU"), DateTimeStyles.NoCurrentDateDefault); // Woolworths likes Australian dates
				result = new ZDateTime(date.Year, date.Month, date.Day);
			}
			return result;
		}

		#endregion
	}
}
