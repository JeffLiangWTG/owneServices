using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public partial class JobCADeclaration : AutoJobCADeclaration, IClusterKeyWorker, Integration.Customs.CA.IJobCADeclaration, IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter
	{
		public JobCADeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(Declaration))]
		public override ZGuid CAD_JE
		{
			get => base.CAD_JE;
			set => base.CAD_JE = value;
		}

		public JobDeclaration Declaration => Factory.Load<JobDeclaration>(CAD_JE);

		public override bool IsSavedByFactory => isPersistent && base.IsSavedByFactory;
		public bool IsPersistent => isPersistent;
		bool isPersistent = true;

		public void MakeNonPersistent()
		{
			isPersistent = false;
		}

		#region IClusterKeyWorker Members

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CAD_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobDeclaration);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CAD_JEInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion

		#region IAddInfoChildUniqueIndexFailureHandlerSupporter Members
		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new AddInfoChildUniqueClusterKeyIndexFailureHandler(this); }
		}

		string IAddInfoChildUniqueIndexFailureHandlerSupporter.UniqueIndexName => JobCADeclarationSchema.Constants.Indexes.FK_UX__CAD_JE;
		IAddInfoChildSupporter IAddInfoChildUniqueIndexFailureHandlerSupporter.Parent => Declaration;
		ZString IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditUser => CAD_SystemLastEditUser;
		ZDateTime IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditTimeUtc => CAD_SystemLastEditTimeUtc;
		#endregion

		#region IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter Members
		string IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter.UniqueClusterIndexName => JobCADeclarationSchema.Constants.Indexes.NR_UC__CAD_ClusterKey;
		IClusterKeyMaster IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter.ClusterKeyMaster => Declaration;
		SchemaIntColumn IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter.ClusterKeyColumn => JobCADeclarationSchema.CAD_ClusterKey;
		#endregion
	}
}
