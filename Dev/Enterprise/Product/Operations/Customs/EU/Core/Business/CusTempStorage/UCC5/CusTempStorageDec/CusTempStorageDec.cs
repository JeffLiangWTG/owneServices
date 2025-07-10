using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(CusTempStorageJobHeader), "CusTempStorageDecs")]
	public class CusTempStorageDec : AutoCusTempStorageDec
		, Integration.Customs.EU.ICusTempStorageDec, ISequenceNumberHeader
	{
		public CusTempStorageDec(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public static readonly CusTempStorageDecTypeDecider TypeDecider = new CusTempStorageDecTypeDecider();

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusTempStorageDecFetchStrategy(this);

		#endregion

		#region STH_SJH

		[RelatedBusinessObject("StorageHeader")]
		public override ZGuid STH_SJH
		{
			get { return base.STH_SJH; }
			set { base.STH_SJH = value; }
		}

		public virtual CusTempStorageJobHeader StorageHeader
		{
			get { return Factory.Load<CusTempStorageJobHeader>(STH_SJH); }
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(CusTempStorageDecLookups.DeclarationStatusList))]
		public override ZString STH_DeclarationStatus { get => base.STH_DeclarationStatus; set => base.STH_DeclarationStatus = value; }

		#region CusTempStorageLines

		[ChildEditable(true)]
		public CusTempStorageLineCollection CusTempStorageLines => cusTempStorageLines ?? (cusTempStorageLines = GetCusTempStorageLines());

		CusTempStorageLineCollection GetCusTempStorageLines()
		{
			var result = CreateNewCusTempStorageLines();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual CusTempStorageLineCollection CreateNewCusTempStorageLines() => new CusTempStorageLineCollection<CusTempStorageLine, CusTempStorageDec>(this);

		CusTempStorageLineCollection cusTempStorageLines;

		#endregion

		#region ISequenceNumberHeader

		public IEnumerable<ISequenceNumberLine> Lines => GetLinesCore();

		protected virtual IEnumerable<ISequenceNumberLine> GetLinesCore()
		{
			return new TypedEnumerable<ISequenceNumberLine>(CusTempStorageLines);
		}

		public HugeSequenceNumberGenerator LineNumberGenerator => lineNumberGenerator ?? (lineNumberGenerator = new HugeSequenceNumberGenerator(this));

		HugeSequenceNumberGenerator lineNumberGenerator;

		#endregion

		#region Delete

		public override void Delete()
		{
			base.Delete();
			this.DeleteChildren<CusEntryNumber>(CusEntryNumSchema.CE_ParentID);
			this.DeleteChildren<CusTempStorageLine>(CusTempStorageLineSchema.TSL_STH);
		}

		#endregion

		#region IsDataEmpty

		public virtual bool IsDataEmpty => false;

		#endregion

		#region Message Collection

		[ChildEditable(true)]
		public EDIMessageCollection Messages => messages ?? (messages = GetLoadedMessagesCollection());
		EDIMessageCollection messages;

		EDIMessageCollection GetLoadedMessagesCollection()
		{
			var result = new EDIMessageCollection(this, Factory);
			result.SetReadOnlyIncludingChildren(true);
			RegisterEditableChildObject(result);
			result.Load();
			return result;
		}

		#endregion
		public ZString PackageType
		{
			get
			{
				var list = CusTempStorageLines.Cast<CusTempStorageLine>().Select(x => x.TSL_PackageType).Distinct();
				return list.Count() <= 1 ? list.FirstOrDefault().ToString() : Multiple;
			}
		}

		public ZInt PackageQty => CusTempStorageLines.Cast<CusTempStorageLine>().Sum(x => x.TSL_PackageQty);

		public ZInt LineCount => CusTempStorageLines.Count;

		public static TCusTempStorageDec Load<TCusTempStorageDec>(CusTempStorageJobHeader parent) where TCusTempStorageDec : CusTempStorageDec
		{
			var query = new ZQuery(CusTempStorageDecSchema.STH_SJH, parent.PK);
			query.AddToFilter(CusTempStorageDecSchema.STH_DeclarationType, parent.SJH_AppCode);
			query.OrderBy = CusTempStorageDecSchema.STH_SystemCreateTimeUtc.Name;
			query.FetchOnlyFromLocalCache = !parent.IsInDatabase;
			query.ReLoadExistingRows = parent.IsInDatabase;
			return parent.Factory.LoadTop1<TCusTempStorageDec>(query);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string Multiple = "Multiple";
	}
}
