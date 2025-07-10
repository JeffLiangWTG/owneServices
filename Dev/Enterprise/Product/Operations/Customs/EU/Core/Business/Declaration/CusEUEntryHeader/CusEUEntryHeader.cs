using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEUEntryHeader : AutoCusEUEntryHeader, Integration.Customs.EU.ICusEUEntryHeader, IClusterKeyWorker, IAddInfoChildUniqueIndexFailureHandlerSupporter
	{
		public CusEUEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region IClusterKeyWorker Implementation

		public Type ParentBizObjType => typeof(CusEntryHeader);

		public ZPropertyInfoGuid FkToParentPty => (ZPropertyInfoGuid)EUH_CHInfo;

		public IEnumerable<ClusterKeyChildInfo> ClusterKeyChildList => null;

		public ZPropertyInfoInt ClusterKeyPty => (ZPropertyInfoInt)EUH_ClusterKeyInfo;

		#endregion

		#region IAddInfoChildUniqueIndexFailureHandlerSupporter Members
		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new AddInfoChildUniqueIndexFailureHandler(this); }
		}

		string IAddInfoChildUniqueIndexFailureHandlerSupporter.UniqueIndexName => CusEUEntryHeaderSchema.Constants.Indexes.FK_UX__EUH_CH;
		IAddInfoChildSupporter IAddInfoChildUniqueIndexFailureHandlerSupporter.Parent => EntryHeader;
		ZString IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditUser => EUH_SystemLastEditUser;
		ZDateTime IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditTimeUtc => EUH_SystemLastEditTimeUtc;
		#endregion

		public CusEntryHeader EntryHeader => Factory.Load<CusEntryHeader>(EUH_CH);

		[RelatedBusinessObject(nameof(EntryHeader))]
		public override ZGuid EUH_CH { get => base.EUH_CH; set => base.EUH_CH = value; }

		public static readonly CusEUEntryHeaderTypeDecider TypeDecider = new CusEUEntryHeaderTypeDecider();
	}
}
