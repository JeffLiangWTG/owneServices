
using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Main.Navigation
{
	public static class ShortcutKeyResources
	{
		static Dictionary<string, MultilingualString> KeyResources { get; } = new Dictionary<string, MultilingualString>();
		static ShortcutKeyResources()
		{
			KeyResources.Add("CTRL", ResString.GetMultilingualString("986fb5af-a9fa-4aea-ae1a-892afc8bc995", "CTRL"));
			KeyResources.Add("ALT", ResString.GetMultilingualString("e6f7e28f-3857-4131-8289-b477eebd3c20", "ALT"));
			KeyResources.Add("SHIFT", ResString.GetMultilingualString("522778f5-ac9b-4198-8c89-1dbe7911b8f4", "SHIFT"));
			KeyResources.Add("SPACE", ResString.GetMultilingualString("6085bf7e-2bbe-4f5f-a4db-a19d7c3eebd5", "SPACE"));
			KeyResources.Add("ENTER", ResString.GetMultilingualString("96a96591-f985-4220-9b69-64c32e94599b", "ENTER"));
			KeyResources.Add("F1", ResString.GetMultilingualString("3779bce0-70f0-4156-80c6-d1a80fc7b808", "F1"));
			KeyResources.Add("F2", ResString.GetMultilingualString("c4b0d1a5-3f8e-4b7f-9c6d-0a1e5f2a3b8f", "F2"));
			KeyResources.Add("F3", ResString.GetMultilingualString("b0f1a2d4-5c8e-4b7f-9c6d-0a1e5f2a3b8f", "F3"));
			KeyResources.Add("F4", ResString.GetMultilingualString("cbf3a893-d628-4f00-bcf7-ae103a0d0c01", "F4"));
			KeyResources.Add("F5", ResString.GetMultilingualString("941bbbdd-c592-454b-9e93-34ab121867e6", "F5"));
			KeyResources.Add("F6", ResString.GetMultilingualString("70444eeb-e5f0-4ccd-8e81-ff41b0e6a414", "F6"));
			KeyResources.Add("F7", ResString.GetMultilingualString("f1a2d4b0-5c8e-4b7f-9c6d-0a1e5f2a3b8f", "F7"));
			KeyResources.Add("F8", ResString.GetMultilingualString("52c696e3-531d-4f74-bd03-4a167b8f187a", "F8"));
			KeyResources.Add("F9", ResString.GetMultilingualString("a62da5a4-8370-4ff0-8709-a000154de3ad", "F9"));
			KeyResources.Add("F10", ResString.GetMultilingualString("bfc73a1c-740d-4723-b969-fdfada24f6de", "F10"));
			KeyResources.Add("F11", ResString.GetMultilingualString("8ea6136b-067e-429c-9fda-c2b0aa8006a9", "F11"));
			KeyResources.Add("F12", ResString.GetMultilingualString("93782681-6a13-4945-911d-7abf3dc1a4e4", "F12"));
			KeyResources.Add("A", ResString.GetMultilingualString("d923aadd-bb27-4bac-b067-8c9f18b57ff1", "A"));
			KeyResources.Add("B", ResString.GetMultilingualString("823c6664-1952-4945-a6c0-139f8d3f9c7e", "B"));
			KeyResources.Add("C", ResString.GetMultilingualString("682d02b9-86a0-4b8c-bec9-13c3c32afc4b", "C"));
			KeyResources.Add("D", ResString.GetMultilingualString("de1b074a-b9b0-4c74-a242-169943dd6644", "D"));
			KeyResources.Add("E", ResString.GetMultilingualString("c1614325-7096-4522-8401-258aaae7041d", "E"));
			KeyResources.Add("F", ResString.GetMultilingualString("21b36e92-e54f-4543-a5bc-b4a392e069af", "F"));
			KeyResources.Add("G", ResString.GetMultilingualString("ea7e9faf-a943-4db9-a24a-ad8f1bc442cf", "G"));
			KeyResources.Add("H", ResString.GetMultilingualString("f41bdcb4-3fda-4dfd-ae48-d2cac7c17097", "H"));
			KeyResources.Add("I", ResString.GetMultilingualString("0f812d40-7953-40b3-9380-96001825a89c", "I"));
			KeyResources.Add("J", ResString.GetMultilingualString("43c96141-1dde-4335-a582-fff0088beb71", "J"));
			KeyResources.Add("K", ResString.GetMultilingualString("81f3f5c6-fcbd-4a94-9cac-1ed98a6b32de", "K"));
			KeyResources.Add("L", ResString.GetMultilingualString("c4dcf28c-ab54-4bd4-a7c8-80c0c4e20a85", "L"));
			KeyResources.Add("M", ResString.GetMultilingualString("4f4228bb-fe4d-4a12-b694-5b4dff13848a", "M"));
			KeyResources.Add("N", ResString.GetMultilingualString("09ae6aa0-77c7-4cde-8ba0-8862418efa7a", "N"));
			KeyResources.Add("O", ResString.GetMultilingualString("09fb1ec5-f8bc-46f7-84a6-8c98c58784d3", "O"));
			KeyResources.Add("P", ResString.GetMultilingualString("5aa9fbfe-46da-4b98-acfd-a52412057be7", "P"));
			KeyResources.Add("Q", ResString.GetMultilingualString("b81ced62-882b-4cf6-bcc5-51776dbb73c8", "Q"));
			KeyResources.Add("R", ResString.GetMultilingualString("9d69725c-7d6d-4881-948e-41ff4bb05ab1", "R"));
			KeyResources.Add("S", ResString.GetMultilingualString("e1becad5-18c5-4cdd-be4c-5747d23f9265", "S"));
			KeyResources.Add("T", ResString.GetMultilingualString("b4ab52c2-36fa-44c2-9cad-4ce75340e240", "T"));
			KeyResources.Add("U", ResString.GetMultilingualString("4a548b81-abd0-40f9-89b6-a92674a6e4ad", "U"));
			KeyResources.Add("V", ResString.GetMultilingualString("7a4971bf-0790-4a94-ad3a-ec5ccb1f9b46", "V"));
			KeyResources.Add("W", ResString.GetMultilingualString("a16dbaa4-159b-4216-a70f-fb8042d675b3", "W"));
			KeyResources.Add("X", ResString.GetMultilingualString("e18bd351-bca4-4797-856c-d9ed27ebcf29", "X"));
			KeyResources.Add("Y", ResString.GetMultilingualString("f78e56db-85af-4753-899c-0987df7d0fe1", "Y"));
			KeyResources.Add("Z", ResString.GetMultilingualString("26587bd8-614c-4c91-918b-b9894d526cf9", "Z"));
			KeyResources.Add("0", ResString.GetMultilingualString("36520e4f-9200-4419-bc5e-546c6d8c34b6", "0"));
			KeyResources.Add("1", ResString.GetMultilingualString("18ff0910-0649-4f34-8633-d73b5b1b56db", "1"));
			KeyResources.Add("2", ResString.GetMultilingualString("eaaf683b-b06e-436b-ba2f-40c32ebc2b44", "2"));
			KeyResources.Add("3", ResString.GetMultilingualString("7ab2cc48-cf51-4394-b1cf-ec5634137f64", "3"));
			KeyResources.Add("4", ResString.GetMultilingualString("f9282319-a43d-4ef6-a33d-bbb9108eda23", "4"));
			KeyResources.Add("5", ResString.GetMultilingualString("662cce44-ea84-4493-bfed-528ee32c8715", "5"));
			KeyResources.Add("6", ResString.GetMultilingualString("2dd234cf-4723-423e-a276-7c02f0b50e5b", "6"));
			KeyResources.Add("7", ResString.GetMultilingualString("b49c4681-0c70-4a93-9ee6-ccd9b94d847b", "7"));
			KeyResources.Add("8", ResString.GetMultilingualString("d139c591-2b80-4230-93d8-36436a3dcab1", "8"));
			KeyResources.Add("9", ResString.GetMultilingualString("ac7fcd0a-0322-42e0-9cf8-be8147be030b", "9"));
			KeyResources.Add("+", ResString.GetMultilingualString("79bae028-3afa-45c6-a984-29011edc147b", "+"));
			KeyResources.Add("-", ResString.GetMultilingualString("ba8f7c45-2f3b-4976-a82b-968c844d3406", "-"));
			KeyResources.Add("=", ResString.GetMultilingualString("7849c134-a42a-41ef-ba25-72fee4f32883", "="));
		}
		static public MultilingualString GetKeyString(string key)
		{
			if (KeyResources.TryGetValue(key.ToUpper(), out var value))
			{
				return value;
			}
			throw new KeyNotFoundException($"Key {key} is not supported! Please add it in static constructor mannually");
		}
	}
}
