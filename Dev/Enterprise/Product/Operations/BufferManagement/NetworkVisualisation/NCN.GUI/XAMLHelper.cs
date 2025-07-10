using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public static class XAMLHelper
	{
		static ResourceDictionary FindAlternativeDataTemplatesResourceDictionary()
		{
			return FindResourceDictionaryByName((NoResString)"pack://application:,,,/Enterprise.BufferManagement.NetworkVisualisation.GUI;component/DataTemplates/AlternativeDataTemplates.xaml"); // Path to XAML element in code
		}

		static ResourceDictionary FindResourceDictionaryByName(ZString name)
		{
			return new ResourceDictionary
			{
				Source = new Uri(name, UriKind.RelativeOrAbsolute)
			};
		}

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		public static AlternativeNodeDataTemplateSelector CreateAndPopulateAlternativeSelector()
		{
			var current = Application.Current; // This is a hack as per http://stackoverflow.com/questions/6005398/uriformatexception-invalid-uri-invalid-port-specified without this when run via command line or GLOW it will always fail with URI invalid port

			var selector = new AlternativeNodeDataTemplateSelector();
			var resourceDictionary = FindAlternativeDataTemplatesResourceDictionary();

			//Buffer Template
			var template = resourceDictionary["bufferNodeDataTemplate"] as DataTemplate; // Path to XAML element in code
			selector.BufferDataTemplate = template;

			return selector;
		}
	}
}
