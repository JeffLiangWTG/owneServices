using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	[SingleObjectAroundARow]
	public partial class JobCAComInvoiceHeader : AutoJobCAComInvoiceHeader, Integration.Customs.CA.IJobCAComInvoiceHeader, IClusterKeyWorker, IAddInfoChildUniqueIndexFailureHandlerSupporter
	{
		public JobCAComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public JobComInvoiceHeader InvoiceHeader => Factory.Load<JobComInvoiceHeader>(CAZ_JZ);

		[RelatedBusinessObject(nameof(InvoiceHeader))]
		public override ZGuid CAZ_JZ
		{
			get => base.CAZ_JZ;
			set => base.CAZ_JZ = value;
		}

		public override bool SupportsNotes => false;

		#region IClusterKeyWorker Members
		Type IClusterKeyWorker.ParentBizObjType => typeof(JobComInvoiceHeader);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CAZ_JZInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;
		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CAZ_ClusterKeyInfo;
		#endregion

		#region IAddInfoChildUniqueIndexFailureHandlerSupporter Members
		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new AddInfoChildUniqueIndexFailureHandler(this); }
		}

		string IAddInfoChildUniqueIndexFailureHandlerSupporter.UniqueIndexName => JobCAComInvoiceHeaderSchema.Constants.Indexes.FK_UX__CAZ_JZ;
		IAddInfoChildSupporter IAddInfoChildUniqueIndexFailureHandlerSupporter.Parent => InvoiceHeader;
		ZString IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditUser => CAZ_SystemLastEditUser;
		ZDateTime IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditTimeUtc => CAZ_SystemLastEditTimeUtc;
		#endregion
	}
}
