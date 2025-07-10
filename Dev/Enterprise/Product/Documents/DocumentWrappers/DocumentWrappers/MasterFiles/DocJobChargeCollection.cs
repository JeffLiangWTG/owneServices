using System;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocJobChargeCollection : DocumentWrapperCollection
	{
		protected DocJobChargeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static DocJobChargeCollection GetCollection(DocumentWrapper parent, string collectionName, Action<DocJobChargeCollection> populateCollection)
			=> CollectionGetter<DocJobChargeCollection, DocJobCharge>.GetCollection(parent, collectionName, populateCollection);

		public static DocJobChargeCollection GetCollection(DocumentWrapper parent, string collectionName, params JobCharge[] jobChargesToAdd)
			=> CollectionGetter<DocJobChargeCollection, DocJobCharge>.GetCollection(parent, collectionName, jobChargesToAdd);

		#region CollectionGetter

		internal static class CollectionGetter<CollectionType, ElementType> where CollectionType : DocJobChargeCollection where ElementType : DocJobCharge
		{
			internal static CollectionType GetCollection(DocumentWrapper parent, string collectionName, Action<CollectionType> populateCollection)
			{
				Argument.NotNull(parent, nameof(parent));
				Argument.NotNullOrEmpty(collectionName, nameof(collectionName));
				Argument.NotNull(populateCollection, nameof(populateCollection));

				return parent.Factory.GetCachedValue(parent.PK.ToStringKey() + collectionName,
					() =>
					{
						var collection = (CollectionType)Activator.CreateInstance(typeof(CollectionType), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, new object[] { parent.Factory }, null);
						collection.PopulateCollection(() => populateCollection(collection));
						return collection;
					},
					CacheStalenessPolicy.StaleWhenDataTableChanges(JobChargeSchema.Constants.TableName, parent.Factory));
			}

			internal static CollectionType GetCollection(DocumentWrapper parent, string collectionName, params JobCharge[] jobChargesToAdd) =>
				GetCollection(parent, collectionName, (collection) =>
				{
					foreach (var jobCharge in jobChargesToAdd)
					{
						var chargeWrapper = GetNewElement(jobCharge, parent.Factory);
						collection.Add(chargeWrapper);
					}
				});

			internal static ElementType GetNewElement(JobCharge jobCharge, BusinessObjectFactory factory) =>
				(ElementType)Activator.CreateInstance(typeof(ElementType), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, new object[] { jobCharge, factory }, null);
		}

		#endregion

		enum DocJobChargeCollectionContext
		{
			PopulatingCollection
		}

		protected void PopulateCollection(Action populateCollection)
		{
			using (Factory.SetTempContext(DocJobChargeCollectionContext.PopulatingCollection))
			{
				populateCollection();
			}
		}

		public override void Add(BusinessObject businessObject)
		{
			if (!Factory.HasContext(DocJobChargeCollectionContext.PopulatingCollection))
			{
				ErrorReporter.ReportOnce("Should not add elements to the DocJobChargeCollection outside of PopulateCollection delegate.");
			}
			base.Add(businessObject);
		}

		public new DocJobCharge this[int index]
		{
			get { return (DocJobCharge)base[index]; }
		}

		public ZBool ChargesAvailable
		{
			get { return Count > 0; }
		}

		public ZDecimal TotalCharges
		{
			get
			{
				ZDecimal result = 0M;
				foreach (DocJobCharge charges in this)
				{
					result += charges.LocalSellAmount;

					if (charges.CurrentCompany != null)
					{
						if (charges.CurrentCompany.IsGSTRegistered)
						{
							result += charges.TaxAmount;
						}
					}
				}
				return result;
			}
		}

		public ZDecimal TotalChargesForIceland
		{
			get
			{
				ZDecimal result = 0M;
				foreach (DocJobCharge charges in this)
				{
					result += charges.LocalSellAmountForIceland;

					if (charges.CurrentCompany != null)
					{
						if (charges.CurrentCompany.IsGSTRegistered)
						{
							result += charges.TaxAmount;
						}
					}
				}
				return result;
			}
		}

		public ZDecimal TotalTaxAmount
		{
			get
			{
				ZDecimal result = 0M;
				if (GlbCompany.CurrentCompany.GC_IsGSTRegistered)
				{
					foreach (DocJobCharge charges in this)
					{
						result += charges.TaxAmount;
					}
				}
				return result;
			}
		}

		public ZDecimal TotalLocalSellAmount
		{
			get
			{
				ZDecimal result = 0M;
				foreach (DocJobCharge charges in this)
				{
					result += charges.LocalSellAmount;
				}
				return result;
			}
		}

		public ZDecimal TotalLocalSellAmountForIceland
		{
			get
			{
				ZDecimal result = 0M;
				foreach (DocJobCharge charges in this)
				{
					result += charges.LocalSellAmountForIceland;
				}
				return result;
			}
		}

		public ZDecimal TotalLocalSellAmountIncTax
		{
			get
			{
				ZDecimal result = 0M;
				foreach (DocJobCharge charges in this)
				{
					result += charges.LocalSellAmountIncTax;
				}
				return result;
			}
		}

		public ZString ChargeSheetOSAmountDisplay
		{
			get
			{
				foreach (DocJobCharge charge in this)
				{
					if (charge.ShowLocalAmountAndExRateOnInvoice)
					{
						return Res.GetString("dc89f82d-da9b-424a-9627-8ff8b65f4009", "OS AMOUNT");
					}
				}
				return ZString.Empty;
			}
		}

		public DocJobChargeCollection DisbursementTypeJobCharges
		{
			get
			{
				var result = new DocJobChargeCollection(Factory);
				result.PopulateCollection(() =>
				{
					foreach (DocJobCharge currentCharge in this)
					{
						if (currentCharge.ChargeCode != null && currentCharge.ChargeCode.ChargeType == Core.Constants.ChargeType.Disbursement)
						{
							result.Add(currentCharge);
						}
					}
				});
				return result;
			}
		}

		public DocJobChargeCollection NonZeroDisbursementJobCharges
		{
			get
			{
				var result = new DocJobChargeCollection(Factory);
				result.PopulateCollection(() =>
				{
					foreach (DocJobCharge currentCharge in DisbursementTypeJobCharges)
					{
						if (!currentCharge.LocalSellAmount.IsEmpty)
						{
							result.Add(currentCharge);
						}
					}
				});
				return result;
			}
		}

		public DocJobChargeCollection NonZeroJobCharges
		{
			get
			{
				var result = new DocJobChargeCollection(Factory);
				result.PopulateCollection(() =>
				{
					foreach (DocJobCharge currentCharge in this)
					{
						if (!currentCharge.LocalSellAmount.IsEmpty)
						{
							result.Add(currentCharge);
						}
					}
				});
				return result;
			}
		}

		public DocJobChargeCollection NonZeroChargesOrEstimatedRevenue
		{
			get
			{
				var result = new DocJobChargeCollection(Factory);
				result.PopulateCollection(() =>
				{
					foreach (DocJobCharge charge in this)
					{
						if (!charge.LocalSellAmount.IsEmpty || !charge.EstimatedRevenue.IsEmpty)
						{
							result.Add(charge);
						}
					}
				});
				return result;
			}
		}

		public DocJobCharge FindByPKOfWrappedObject(ZGuid pK)
		{
			DocJobCharge result = null;
			foreach (DocJobCharge charge in this)
			{
				if (((JobCharge)charge.WrappedObject).PK == pK)
				{
					result = charge;
					break;
				}
			}
			return result;
		}
	}
}
