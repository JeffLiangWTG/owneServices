using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ReflectiveFieldMap;

namespace Enterprise.DocumentEngine.ValueProviders
{
	/// <summary>
	/// The data source for the MapTreeForm.
	/// </summary>
	public class DataReflectorValueProviderWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DataReflectorValueProviderWrapper(DocDataProviderReflector[] docDataProviderReflectors, ValueProviderMap valueProviderMap, Mode operationsMode)
		{
			this.DocDataProviderReflectors = docDataProviderReflectors;
			this.ValueProviderMap = valueProviderMap;
			this.OperationsMode = operationsMode;
			this.VariableReflectors = Array.Empty<DocDataProviderReflector>();
		}

		public DataReflectorValueProviderWrapper(DocDataProviderReflector[] docDataProviderReflectors, DocDataProviderReflector xmlReflector, ValueProviderMap valueProviderMap, Mode operationsMode)
			: this(docDataProviderReflectors, valueProviderMap, operationsMode)
		{
			XmlReflector = xmlReflector;
		}

		public DataReflectorValueProviderWrapper(DocDataProviderReflector[] docDataProviderReflectors, string[] variableNames, DocDataProviderReflector[] variableReflectors, DocDataProviderReflector xmlReflector, ValueProviderMap valueProviderMap, Mode operationsMode)
			: this(docDataProviderReflectors, valueProviderMap, operationsMode)
		{
			VariableNames = variableNames;
			VariableReflectors = variableReflectors;
			XmlReflector = xmlReflector;
		}

		public readonly Mode OperationsMode;
		public enum Mode { Browse, Select }

		public IReadOnlyList<DocDataProviderReflector> DocDataProviderReflectors
		{
			get;
			private set;
		}

		public string[] VariableNames;

		public IReadOnlyList<DocDataProviderReflector> VariableReflectors { get; private set; }

		public DocDataProviderReflector XmlReflector { get; private set; }

		public ValueProviderMap ValueProviderMap
		{
			get;
			private set;
		}

		public string SelectedMacro
		{
			get;
			set;
		}

		public int SelectedDocDataProviderReflector
		{
			get;
			set;
		}

		public ZString TopLevelDataSourceInformation
		{
			get { return DocDataProviderReflectors[SelectedDocDataProviderReflector].TopLevelDataSourceInformation; }
		}

		public ZPropertyInfo TopLevelDataSourceInformationInfo
		{
			get { return GetZPropertyInfo(nameof(TopLevelDataSourceInformation)); }
		}

		public ZString SelectedMemberInformation
		{
			get { return selectedMemberInformation; }
		}
		ZString selectedMemberInformation = NoMemberSelected;

		public ZPropertyInfo SelectedPropertyInformationInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedMemberInformation)); }
		}

		internal static string NoMemberSelected
		{
			get { return Res.GetString("c6be2c86-4556-44a4-b3c2-a049cbca3066", "(none selected)"); }
		}

		public void SetSelectedProperty(MemberDescription currentMember)
		{
			if (selectedMember != currentMember)
			{
				selectedMemberInformation = currentMember == null ? NoMemberSelected : currentMember.GetMemberInformation();
				SelectedPropertyInformationInfo.RefreshBinding();
				selectedMember = currentMember;
			}
		}

		MemberDescription selectedMember;
	}
}
