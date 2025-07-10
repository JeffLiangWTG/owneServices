using System;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	sealed class DocDataObjectOnValueChangedActionBuilder : IDocDataObjectOnValueChangedActionBuilder
	{
		public DocDataObjectOnValueChangedActionBuilder(DocDataObject docDataObject, string propertyName)
		{
			this.docDataObject = docDataObject ?? throw new ArgumentNullException(nameof(docDataObject));
			this.propertyName = propertyName;
		}

		readonly DocDataObject docDataObject;
		readonly string propertyName;

		public void Validate(params string[] propertyNames)
		{
			if (propertyNames == null)
			{
				return;
			}

			if (docDataObject is IValueChangedActionSupporter supporter)
			{
				supporter.AddOnValueChangedAction(propertyName, () => docDataObject.Validate(propertyNames));
			}
		}

		public void Do(Action action)
		{
			if (action != null
				&& docDataObject is IValueChangedActionSupporter supporter)
			{
				supporter.AddOnValueChangedAction(propertyName, action);
			}
		}
	}
}
