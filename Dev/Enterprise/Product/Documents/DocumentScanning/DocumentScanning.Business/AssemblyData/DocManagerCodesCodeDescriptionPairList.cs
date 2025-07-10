using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.DocumentScanning.Business
{
	public sealed class DocManagerCodesCodeDescriptionPairList : IEnumerable<KeyValuePair<string, IAssemblyData>>
	{
		internal DocManagerCodesCodeDescriptionPairList(IGlbCompany company)
		{
			Argument.NotNull(company, "company");
			dictionary = new Dictionary<string, IAssemblyData>();
			Company = company;
			CountryCode = Company.GC_RN_NKCountryCode;
		}

		internal DocManagerCodesCodeDescriptionPairList()
		{
			dictionary = new Dictionary<string, IAssemblyData>();
		}

		readonly Dictionary<string, IAssemblyData> dictionary;

		internal string CountryCode { get; private set; }
		internal IGlbCompany Company { get; private set; }

		public IAssemblyData Unallocated
		{
			get { return dictionary[Core.Constants.DocManagerCodes.Unallocated]; }
		}

		public string GetDescriptionFromCode(string code)
		{
			return dictionary[code].HumanReadableName;
		}

		public IAssemblyData GetAssemblyDataFromDocManagerCode(string docManagerCode)
		{
			IAssemblyData result;
			dictionary.TryGetValue(docManagerCode, out result);
			return result;
		}

		public string GetReferenceTypeFromDocManagerCode(string docManagerCode)
		{
			string result = null;
			IAssemblyData assemblyData;
			if (dictionary.TryGetValue(docManagerCode, out assemblyData))
			{
				result = assemblyData.ReferenceType;
			}
			return result;
		}

		internal void Replace(string code, IAssemblyData assemblyDataHolder)
		{
			dictionary[code] = assemblyDataHolder;
		}

		internal void Add(string code, IAssemblyData assemblyDataHolder)
		{
			dictionary.Add(code, assemblyDataHolder);
		}

		internal IEnumerable<string> GetDocManagerCodes()
		{
			return dictionary.Keys;
		}

		#region IEnumerable<KeyValuePair<string,IAssemblyData>> Members

		public IEnumerator<KeyValuePair<string, IAssemblyData>> GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		#endregion
	}
}
