export interface Endpoint {
    name: string,
	indexPath: string,
	messageBoxPath: string,
}

export interface Message {
	name: string,
	created: Date,
	modified: Date,
	size: number,
	content?: string,
	status: Status
}

export interface Folder {
	name: string,
	created: Date,
	modified: Date,
	size: number
}

export enum Status {
	Opening,
	Editing,
	Saving,
	Closed
}

export interface AppSatus {
	busy: boolean,
	message: string
}

export interface MockEndpointInfo {
	interfaceName: string,
	controllerName: string,
	checkRunning: string,
	optionalHtml: string
}