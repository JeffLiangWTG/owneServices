using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class StatisticsFoldupInfoCollection : RegistryBusinessObjectCollectionTemplate
	{
		public StatisticsFoldupInfoCollection() : base() { }
		public StatisticsFoldupInfoCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory) { }

		public new StatisticsFoldupInfo this[int i]
		{
			get { return (StatisticsFoldupInfo)Elements[i]; }
		}

		protected override bool AllowSort
		{
			get { return false; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var added = bizOAdded as StatisticsFoldupInfo;
			if (added != null && added.ParentCollection == null)
			{
				added.ParentCollection = this;
			}
		}

		public new StatisticsFoldupInfo AddNew()
		{
			return (StatisticsFoldupInfo)base.AddNew();
		}

		public StatisticsFoldupInfo AddNew(string waitScale, int waitAmount, string foldScale, int foldAmount)
		{
			var rule = this.AddNew();
			using (rule.GetValidationSuspender())
			{
				rule.WaitScale = waitScale;
				rule.WaitAmount = waitAmount;
				rule.AggregateScale = foldScale;
				rule.AggregateAmount = foldAmount;
			}

			return rule;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StatisticsFoldupInfo(CurrentFallbackLevel, CurrentFactory, this);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new StatisticsFoldupInfoCollection(fallbackLevel, factory);
		}

		#region DefaultCollection

		public static StatisticsFoldupInfoCollection GetDefaultCollection()
		{
			var collection = new StatisticsFoldupInfoCollection();
			collection.AddNew(TimeFrameList.Codes.Day, 1, TimeFrameList.Codes.Minute, 15);
			collection.AddNew(TimeFrameList.Codes.Month, 1, TimeFrameList.Codes.Hour, 1);
			collection.AddNew(TimeFrameList.Codes.Year, 1, TimeFrameList.Codes.Day, 1);
			collection.AddNew(TimeFrameList.Codes.Year, 2, TimeFrameList.Codes.Month, 1);

			return collection;
		}

		#endregion //DefaultCollection
	}
}
