using System;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	/// <summary>
	/// Listing of all modules in Enterprise for Licence Key Generation
	/// </summary>
	public class LicenceModuleList
	{
		#region Singleton Construction

		public static LicenceModuleList Instance
		{
			get { return instance ?? (instance = new LicenceModuleList()); }
		}

		[ThreadStatic]
		static LicenceModuleList instance;

		#endregion

		public string GetDescriptionFromCode(string code)
		{
			return Names.GetDescriptionFromCode(code);
		}

		/// <summary>
		/// Licence modules - excludes Language Licence Child checkpoints
		/// </summary>
		public CodeDescriptionPairList Names
		{
			get { return names ?? (names = CreateNames()); }
		}
		CodeDescriptionPairList names;

		/// <summary>
		/// Licence modules - includes Language Licence Child checkpoints
		/// </summary>
		public CodeDescriptionPairList NamesIncludingChildren
		{
			get { return namesIncludingChildren ?? (namesIncludingChildren = CreateNamesIncludingChildren()); }
		}
		CodeDescriptionPairList namesIncludingChildren;

		LegacyLicence LicenceObject
		{
			get { return licenceObject ?? (licenceObject = new LegacyLicence()); }
		}
		LegacyLicence licenceObject;

		void BuildNames(CodeDescriptionPairList list)
		{
			foreach (var checkPoint in LicenceObject.GetAllModuleCheckpoints())
			{
				list.AddPair(checkPoint.Name, checkPoint.DisplayName);
			}
		}

		CodeDescriptionPairList CreateNames()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			BuildNames(list);
			return list;
		}

		CodeDescriptionPairList CreateNamesIncludingChildren()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			foreach (var checkPoint in LicenceObject.GetAllCheckpoints())
			{
				list.AddPair(checkPoint.Name, checkPoint.DisplayName);
			}
			return list;
		}

		public string GetDescription(string moduleCode)
		{
			return NamesIncludingChildren.GetDescriptionFromCode(moduleCode);
		}

		#region Indentation

		public string GetIndentedDescriptionFromCode(string moduleCode)
		{
			string desc = NamesIncludingChildren.GetDescriptionFromCode(moduleCode);
			return desc != null ? desc.PadLeft(desc.Length + GetIndentLevel(moduleCode, 0), IndentCharacter) : "Module No Longer Valid";
		}

		int GetIndentLevel(string moduleCode, int currentLevel)
		{
			string parentModule = LicenceObject.GetCheckpointFromCode(moduleCode).ParentModule;
			if (parentModule != null && parentModule.Length != 0)
			{
				currentLevel = IndentLevel + GetIndentLevel(parentModule, currentLevel);
			}

			return currentLevel;
		}

		public string GetCodeOfModulesParent(string moduleCode)
		{
			string code = "";
			var checkpoint = LicenceObject.GetCheckpointFromCode(moduleCode);
			if (LicenceObject != null && checkpoint != null)
			{
				code = checkpoint.ParentModule;
			}
			return code;
		}

		public const int IndentLevel = 5;
		public const char IndentCharacter = ' ';

		#endregion
	}
}
