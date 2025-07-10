using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobEUDeclaration : AutoJobEUDeclaration, Integration.Customs.EU.IJobEUDeclaration, IClusterKeyWorker, IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter
	{
		public JobEUDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public JobDeclaration Declaration => (JobDeclaration)Factory.Load(JobDeclarationType, EUD_JE);

		protected virtual Type JobDeclarationType => typeof(JobDeclaration);

		[RelatedBusinessObject(nameof(Declaration))]
		public override ZGuid EUD_JE { get => base.EUD_JE; set => base.EUD_JE = value; }

		protected override IValueSetStrategy GetValueSetStrategy()
		{
			return new JobEUDeclarationSetStrategy(this);
		}

		#region IClusterKeyWorker Implementation

		public Type ParentBizObjType => typeof(JobDeclaration);

		public ZPropertyInfoGuid FkToParentPty => (ZPropertyInfoGuid)EUD_JEInfo;

		public IEnumerable<ClusterKeyChildInfo> ClusterKeyChildList => null;

		public ZPropertyInfoInt ClusterKeyPty => (ZPropertyInfoInt)EUD_ClusterKeyInfo;

		#endregion

		#region IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter Members
		string IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter.UniqueClusterIndexName => JobEUDeclarationSchema.Constants.Indexes.NR_UC__EUD_ClusterKey;

		IClusterKeyMaster IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter.ClusterKeyMaster => Declaration;

		SchemaIntColumn IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter.ClusterKeyColumn => JobEUDeclarationSchema.EUD_ClusterKey;

		string IAddInfoChildUniqueIndexFailureHandlerSupporter.UniqueIndexName => JobEUDeclarationSchema.Constants.Indexes.FK_UX__EUD_JE;

		IAddInfoChildSupporter IAddInfoChildUniqueIndexFailureHandlerSupporter.Parent => Declaration;

		ZString IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditUser => EUD_SystemLastEditUser;

		ZDateTime IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditTimeUtc => EUD_SystemLastEditTimeUtc;
		#endregion

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new AddInfoChildUniqueClusterKeyIndexFailureHandler(this); }
		}

		public override bool IsSavedByFactory => isPersistent && base.IsSavedByFactory;
		public bool IsPersistent => isPersistent;
		bool isPersistent = true;

		[List(nameof(Lookups) + "." + nameof(JobEUDeclarationLookups.AgreedPlaceCodeList))]
		[ResourceStringData("E4F4A12C-243E-4BC3-848F-47C8E2072C17", Caption = "Incoterm Place Code", MediumCaption = "Inco. Place Code", ShortCaption = "Inco. Place Code", FullDescription = "Incoterm Place Code: insert a Country (2 chars) or an UNLOCO (5 chars)")]
		public override ZString EUD_AgreedPlaceCode
		{
			get => base.EUD_AgreedPlaceCode;
			set
			{
				var oldValue = EUD_AgreedPlaceCode;
				base.EUD_AgreedPlaceCode = value;
				if (!IsCopying && oldValue != EUD_AgreedPlaceCode)
				{
					if (!EUD_AgreedPlaceCode.IsEmpty)
					{
						if (Declaration.IsAgreedUnloco && Declaration.AgreedPlaceCodeSupport)
						{
							Declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
						}
						else
						{
							if (Declaration.ZG_AgreedPlaceCode.IsEmpty && Declaration.AddInfoLookups.AgreedPlaceCodeList is CodeDescriptionPairList codeDescriptionPairList)
							{
								Declaration.ZG_AgreedPlaceCode = codeDescriptionPairList.GetAllCodes().FirstOrDefault();
							}
						}
					}
					Declaration.MarkAsNeedingValidation();
				}
			}
		}

		public void MakeNonPersistent()
		{
			isPersistent = false;
		}
	}
}
