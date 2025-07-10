using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using ColorList = CargoWise.NetworkVisualisation.Business.ColorList;

[assembly: ResourceStringDataCaptionSource(typeof(ColorList))]
namespace CargoWise.NetworkVisualisation.Business
{
	public class ColorList : ImpObservableCollection<NamedColor>, ICustomizableDataCaptionSource
	{
		public ColorList()
		{
			foreach (var tuple in Enterprise.ZArchitecture.Core.ColorList.GetKnownColors())
			{
				Add(new NamedColor(tuple.HumanReadableName, Color.FromName(tuple.ColorName)));
			}
		}

		public CodeDescriptionPairList GetTranslatedColorNames()
		{
			if (translatedNameColors == null)
			{
				translatedNameColors = new CodeDescriptionPairList();
				translatedNameColors.AddRange(this.Select(e => new CodeDescriptionPair(e.Name, e.TranslatedName)).ToArray());
			}
			return translatedNameColors;
		}
		CodeDescriptionPairList translatedNameColors;

		#region IResourceStringAnalyzerSource member

		string ICustomizableDataCaptionSource.GetKey(object context, string caption)
		{
			throw new NotImplementedException();
		}

		IEnumerable<IResString> ICustomizableDataCaptionSource.GetCompileTimeSystemCaptions()
		{
			return this.Select(x => x.GetResourceStringCaption());
		}

		IEnumerable<IResString> ICustomizableDataCaptionSource.GetRuntimeCaptions(IResString userCaption, object context)
		{
			throw new NotImplementedException();
		}

		string ICustomizableDataCaptionSource.Description => throw new NotImplementedException();

		int ICustomizableDataCaptionSource.MaxLength => 0;

		ushort ICustomizableDataCaptionSource.Asmid
		{
			get => NamedColor.NamedColorAssemblyId;
			set => throw new NotImplementedException();
		}

		#endregion
	}
}
