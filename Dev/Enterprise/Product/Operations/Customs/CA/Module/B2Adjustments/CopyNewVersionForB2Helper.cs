using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public static class CopyNewVersionForB2Helper
	{
		public static JobDeclaration CopyToNewVersionForB2(JobDeclaration sourceDeclaration, bool canCopyFromIMP = false)
		{
			JobDeclaration result = null;
			if (sourceDeclaration.IsCancelled)
			{
				Globals.Message.ShowInformation(OnlyCopyFromActiveJobMessage, CopyToNewVersionForB2Caption);
			}
			else
			{
				result = CopyForIMPAndIM2Declaration(sourceDeclaration, canCopyFromIMP);
			}

			return result;
		}

		static JobDeclaration CopyForIMPAndIM2Declaration(JobDeclaration sourceDeclaration, bool canCopyFromIMP)
		{
			JobDeclaration result = null;
			if ((!canCopyFromIMP || sourceDeclaration.JE_MessageType != JobMessageTypeList.Codes.Import) && !sourceDeclaration.IsIM2)
			{
				Globals.Message.ShowInformation(OnlyCopyFromIMPOrIM2Message, CopyToNewVersionForB2Caption);
			}
			else
			{
				if (canCopyFromIMP
					&& sourceDeclaration.JE_MessageType == JobMessageTypeList.Codes.Import
					&& !sourceDeclaration.CA_K84AccountingDate.IsValid)
				{
					Globals.Message.ShowInformation(OnlyCopyForAccountedJobsMessage, CopyToNewVersionForB2Caption);
				}
				else if (sourceDeclaration.IsIM2 && !sourceDeclaration.CA_B2AcceptedDate.IsValid)
				{
					Globals.Message.ShowInformation(OnlyCopyForAcceptedJobsMessage, CopyToNewVersionForB2Caption);
				}
				else
				{
					result = GetNewCopyToB2Declaration(sourceDeclaration);
				}
			}

			return result;
		}

		static JobDeclaration GetNewCopyToB2Declaration(JobDeclaration sourceDeclaration)
		{
			JobDeclaration result = null;
			if (IsNextVersionNumberDeclarationExist(sourceDeclaration))
			{
				Globals.Message.ShowInformation(NewVersionAlreadyExistsMessage, CopyToNewVersionForB2Caption);
			}
			else
			{
				result = sourceDeclaration.GetNewCopyToB2Declaration();
			}

			return result;
		}

		static bool IsNextVersionNumberDeclarationExist(JobDeclaration sourceDeclaration)
		{
			var nextVersionDeclaration = sourceDeclaration.GetNextVersionNumberDeclaration();

			return nextVersionDeclaration != null
				&& !nextVersionDeclaration.IsCancelled
				&& nextVersionDeclaration.CA_Version != sourceDeclaration.CA_Version;
		}

		static string NewVersionAlreadyExistsMessage
		{
			get { return Res.GetString("399f1b23-588e-45ad-b849-14d108a8bd80", "The new version already exists. If you wish to ignore the existing version then open that version and deactivate it by selecting Actions=>Mark Inactive, then retry the copy to a new version."); }
		}

		static string OnlyCopyForAccountedJobsMessage
		{
			get { return Res.GetString("b927e796-1aa6-4c78-9832-a6e7d14f166e", "You may only copy declarations that have been accounted for."); }
		}

		static string OnlyCopyForAcceptedJobsMessage
		{
			get { return Res.GetString("bbdcca69-2eba-49ec-a34f-a95d03217400", "You may only copy declarations that have been accepted (decided)."); }
		}

		static string OnlyCopyFromIMPOrIM2Message
		{
			get { return Res.GetString("26741833-59D1-49F2-85A7-044C9CCFDC7F", "You may only copy Import or other Import Copy for B2 type declarations to a new version for B2."); }
		}

		static string OnlyCopyFromActiveJobMessage
		{
			get { return Res.GetString("6f1b7de3-3d3b-4b6e-afa5-e127ba0de4b8", "You may only copy active declarations to a new version for B2."); }
		}

		internal static string CopyToNewVersionForB2Caption
		{
			get { return Res.GetString("43d5162a-d5d2-456a-9ea8-6a6e8821506e", "Copy to new version for B2"); }
		}

		internal static string CopyNotAllowedForIM2Message
		{
			get { return Res.GetString("67a8839d-0ffc-46f9-a7f8-f3e72fd5d646", "The normal template copy is not allowed for an IM2 declaration."); }
		}
	}
}
