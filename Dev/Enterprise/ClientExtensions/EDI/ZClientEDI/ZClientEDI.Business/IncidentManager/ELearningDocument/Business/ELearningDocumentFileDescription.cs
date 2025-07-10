using System;
using Newtonsoft.Json;

namespace ZClientEDI.Business.IncidentManager.ELearningDocument.Business
{
	public interface IELearningDocumentFileDescription
	{
		string Id { get; set; }
		string Title { get; set; }
		string Url { get; set; }
		string Type { get; set; }
		IELearningDocumentDescription ToMetaDocument();
	}

	public class ELearningDocumentFileDescription : IELearningDocumentFileDescription
	{
		string id;

		[JsonProperty("id")]
		public string Id
		{
			get { return id; }
			set
			{
				CheckValue(nameof(id), value); 
				id = value;
			}
		}

		string title;

		[JsonProperty("title")]
		public string Title
		{
			get { return title; }
			set
			{
				CheckValue(nameof(title), value); 
				title = value;
			}
		}

		string url;

		[JsonProperty("url")]
		public string Url
		{
			get { return url; }
			set
			{
				CheckValue(nameof(url), value); 
				url = value;
			}
		}

		string type;

		[JsonProperty("type")]
		public string Type
		{
			get { return type; }
			set
			{
				CheckValue(nameof(type), value); 
				type = value;
			}
		}

		public IELearningDocumentDescription ToMetaDocument()
		{
			return new ELearningDocumentDescription(null, null)
			{
				MyAccountDocumentId = new Guid(Id),
				DocumentType = Type,
				Title = Title,
				Url = Url
			};
		}

		void CheckValue(string name, string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(name, $"{name} should have a value");
			}
			if (value.Trim().Length == 0)
			{
				throw new ArgumentException($"{name} should have non empty value", name);
			}
		}
	}
}
