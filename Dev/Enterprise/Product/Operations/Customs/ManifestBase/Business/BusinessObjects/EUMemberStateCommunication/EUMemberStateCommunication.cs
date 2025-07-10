using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ManifestBase
{
	public class EUMemberStateCommunication : AutoEUMemberStateCommunication
		, Integration.Customs.ManifestBase.IEUMemberStateCommunication, IClusterKeyWorker
	{
		public EUMemberStateCommunication(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ThreadSafe]
		public static readonly EUMemberStateCommunicationTypeDecider TypeDecider = new EUMemberStateCommunicationTypeDecider();

		public BusinessObject Parent => base.Factory.Load(EUS_ParentTableCode, EUS_ParentId);

		#region IClusterKeyWorker
		public Type ParentBizObjType
		{
			get
			{
				switch (EUS_ParentTableCode)
				{
					case AsycudaManifestHeaderSchema.Constants.Prefix:
						return typeof(AsycudaManifestHeader);
					default:
						return null;
				}
			}
		}

		public ZPropertyInfoGuid FkToParentPty => (ZPropertyInfoGuid)EUS_ParentIdInfo;

		public IEnumerable<ClusterKeyChildInfo> ClusterKeyChildList => null;

		public ZPropertyInfoInt ClusterKeyPty => (ZPropertyInfoInt)EUS_ClusterKeyInfo;
		#endregion
	}
}
