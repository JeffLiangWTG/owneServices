using System.Collections.Generic;
using CargoWise.Definitions;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(NoteTypeToXmlCodeMappings))]
	sealed class NoteTypeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		public override void TestNoNewCodesAdded()
		{
			var mappings = GetNewMappings();
			var unmappedEnterpriseCodes = new Dictionary<string, object>();

			foreach (var type in PredefinedNoteTypes.Instance.All)
			{
				if (!type.IsCustomNoteType && type.DefaultVisibility == StmNoteVisibility.PUB)
				{
					unmappedEnterpriseCodes[type.Description] = null;
				}
			}

			foreach (var mapping in mappings)
			{
				unmappedEnterpriseCodes.Remove(mapping.EnterpriseCode);
			}

			var result = new Dictionary<string, IList<string>>();

			if (unmappedEnterpriseCodes.Count > 0)
			{
				var list = new List<string>(unmappedEnterpriseCodes.Keys);
				list.Sort();

				result.Add("These note types are not mapped", list);
			}

			AssertGroupedErrorList(result);
		}

		#region Implementation

		protected override System.Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return System.Array.Empty<System.Type>(); }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return false; }
		}

		#endregion
	}
}
