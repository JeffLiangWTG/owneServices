import { Component } from '@angular/core';
import { EndpointService } from "../../shared/endpoint.service";
import { Endpoint } from '../../model/messaging-interface';


@Component({
	selector: 'protocols',
	templateUrl: './protocols.component.html',
	styleUrls: ['./protocols.component.css'],
	providers: [EndpointService]
})

export class ProtocolsComponent {
	error: any;
	restEndpoints: Endpoint[] = [];
	soapEndpoints: Endpoint[] = [];
	wcfEndpoints: Endpoint[] = [];
	ftpEndpoints: Endpoint[] = [];
	sftpEndpoints: Endpoint[] = [];
	pop3Endpoints: Endpoint[] = [];
	smtpEndpoints: Endpoint[] = [];
	systemEndpoints: Endpoint[] = [];
	searchText = "";
	constructor(endpointService: EndpointService) {
		endpointService.getEndpoints('rest').then(data => this.restEndpoints = data.endpoints);
        endpointService.getEndpoints('soap').then(data => this.soapEndpoints = data.endpoints);
		endpointService.getEndpoints('wcf').then(data => this.wcfEndpoints = data.endpoints);
		endpointService.getEndpoints('pop3').then(data => this.pop3Endpoints = data.endpoints);
		endpointService.getEndpoints('smtp').then(data => this.smtpEndpoints = data.endpoints);
		endpointService.getEndpoints('ftp').then(data => this.ftpEndpoints = data.endpoints);
		endpointService.getEndpoints('sftp').then(data => this.sftpEndpoints = data.endpoints);
		endpointService.getEndpoints('system').then(data => this.systemEndpoints = data.endpoints);
	}

	toggleProtocol(collapse: string) {
		let els = document.getElementsByClassName("btn btn-toggle");
		Array.prototype.forEach.call(els, function(el) {
			if (el.ariaExpanded && el.ariaExpanded === collapse){
				el.click();
			}
		});
	}
}
