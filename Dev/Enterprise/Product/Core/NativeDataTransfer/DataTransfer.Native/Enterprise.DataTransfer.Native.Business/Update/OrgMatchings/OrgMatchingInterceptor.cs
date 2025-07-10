using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.Utils.Models;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.OrgMatching;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings
{
	public class OrgMatchingInterceptor : BaseInterceptor
	{
		public OrgMatchingInterceptor(OrgMatchingSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
			factory = setting.Context.ObjectFactory;
			orgMatchingConverter = new EntityToOrgMatchingConverter(factory);
			unmatchedRecordConverter = new EntityToUnmatchOrgRecordConverter();
		}
		readonly BusinessObjectFactory factory;
		readonly EntityToOrgMatchingConverter orgMatchingConverter;
		readonly EntityToUnmatchOrgRecordConverter unmatchedRecordConverter;

		#region SuppressResourceStringsCheckRegion

		public override void Invoke(IEntitySet entitySet)
		{
			var root = entitySet.Root;

			if (entitySet.Name == "Organization")
			{
				Function(entitySet);
				CreateNotes(root);
				return;
			}

			var organizations = FindOrgHeaderEntities(root);
			foreach (var orgHeader in organizations)
			{
				MatchOrg(orgHeader, root);
			}

			Function(entitySet);
			CreateNotes(root);
		}

		IEnumerable<IEntity> FindOrgHeaderEntities(IEntity root)
		{
			return root.Relatives().Where(p => p.TableName == "OrgHeader");
		}

		#endregion

		public void MatchOrg(IEntity entity, IEntity order)
		{
			var orgMatchingData = orgMatchingConverter.Convert(entity);
			try
			{
				var orgHeader = new OrganisationMatcher(factory, IfUnmatched.ReturnUnmatchedOrganisation).GetMatchingOrganization(orgMatchingData);

				AssertNotNull(orgHeader);
				entity.InternalPK = orgHeader.PK.ToGuid();
				entity["Code"] = orgHeader.OH_Code;
				entity.ChildrenCollection.RemoveAll();

				if (orgHeader.PK == OrgHeader.UnmatchedOrganisationPK)
				{
					var record = unmatchedRecordConverter.Convert(orgMatchingData);
					order.AddNote(record);
				}
			}
			finally
			{
				orgMatchingData.Delete();
			}
		}

		public void CreateNotes(IEntity entity)
		{
			var noteCreater = new UnmatchNoteCreator(factory);

			foreach (var note in entity.Notes)
			{
				noteCreater.Create(entity, note);
			}
			factory.Save();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		void AssertNotNull(OrgHeader orgHeader)
		{
			if (orgHeader == null)
			{
				const string errorMsg =
					@"You should not be able to reach here, check logic in Match By Default Value. \r\n Organisation Matching should always match an Organisation";
				throw new ArgumentOutOfRangeException(errorMsg);
			}
		}
	}
}
