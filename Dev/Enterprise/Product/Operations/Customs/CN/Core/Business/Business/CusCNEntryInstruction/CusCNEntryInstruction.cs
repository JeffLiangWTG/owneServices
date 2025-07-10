using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.CN;

namespace Enterprise.Customs.CN.Business
{
	public class CusCNEntryInstruction : AutoCusCNEntryInstruction, ICusCNEntryInstruction, IClusterKeyWorker, IAddInfoChildUniqueIndexFailureHandlerSupporter
	{
		public CusCNEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region IClusterKeyWorker Implementation

		public Type ParentBizObjType => typeof(CusEntryInstruction);

		public ZPropertyInfoGuid FkToParentPty => (ZPropertyInfoGuid)CNE_CEIInfo;

		public IEnumerable<ClusterKeyChildInfo> ClusterKeyChildList => null;

		public ZPropertyInfoInt ClusterKeyPty => (ZPropertyInfoInt)CNE_ClusterKeyInfo;

		#endregion

		#region Implementation

		public override bool SupportsNotes => false;

		public CusEntryInstruction EntryInstruction => Factory.Load<CusEntryInstruction>(CNE_CEI);

		[RelatedBusinessObject(nameof(EntryInstruction))]
		public override ZGuid CNE_CEI
		{
			get => base.CNE_CEI;
			set => base.CNE_CEI = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusCNEntryInstructionLookups.TransitionSiteList))]
		public override ZString CNE_TransitionSite { get => base.CNE_TransitionSite; set => base.CNE_TransitionSite = value; }

		public override ZBool CNE_ApplyForTransition
		{
			get => base.CNE_ApplyForTransition;
			set
			{
				base.CNE_ApplyForTransition = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCNE_TransitionSite();
				}
			}
		}

		#endregion

		#region IAddInfoChildUniqueIndexFailureHandlerSupporter Members
		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new AddInfoChildUniqueIndexFailureHandler(this); }
		}

		string IAddInfoChildUniqueIndexFailureHandlerSupporter.UniqueIndexName => CusCNEntryInstructionSchema.Constants.Indexes.FK_UX__CNE_CEI;
		IAddInfoChildSupporter IAddInfoChildUniqueIndexFailureHandlerSupporter.Parent => EntryInstruction;
		ZString IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditUser => EntryInstruction?.CEI_SystemLastEditUser ?? ZString.Empty;
		ZDateTime IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditTimeUtc => EntryInstruction?.CEI_SystemLastEditTimeUtc ?? ZDateTime.Empty;
		#endregion
	}
}
